namespace FollowMotors.Infrastructure.Operaciones;

using System.Data;
using Dapper;
using FollowMotors.Application.Operaciones;
using FollowMotors.Domain.Rules.Operaciones;
using FollowMotors.Domain.Rules.Cancelaciones;
using FollowMotors.Infrastructure.Database;
using MySqlConnector;

/// <summary>Solicitudes de cancelación, garantías, mantenimientos y bandeja local de notificaciones.</summary>
public sealed partial class OperacionesTallerMySql
{
    /// <summary>Guarda una solicitud idempotente, aplicando el umbral desde el primer intento.</summary>
    public async Task<ulong> SolicitarCancelacion(ulong ordenId, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var orden = await BloquearOrden(db, tx, ordenId, ct);
        if (orden is null || (actor.Rol == "CLIENTE" && orden.TitularUsuarioId != actor.UsuarioId))
            throw new KeyNotFoundException("No existe la orden.");
        ulong? existente = await db.ExecuteScalarAsync<ulong?>(new CommandDefinition("""
            SELECT id FROM solicitudes_cancelacion WHERE orden_id = @ordenId
            """, new { ordenId }, tx, cancellationToken: ct));
        if (existente.HasValue) return existente.Value;
        if (!orden.Abierta || orden.Pagado is null)
            throw new InvalidOperationException("Solo se puede solicitar cancelar una reparación contratada y abierta.");
        decimal avance = Avance(await Tareas(db, tx, orden.PresupuestoId, ct));
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO solicitudes_cancelacion (orden_id,cliente_id) VALUES (@Id,@ClienteId)
            """, orden, tx, cancellationToken: ct));
        ulong id = await ConsultasMySql.IdNuevoAsync(db, tx, ct);
        await Auditar(db, tx, actor, "solicitudes_cancelacion", id, "SOLICITAR", ct);
        if (!PoliticaCancelacion.AvanceCancelable(avance))
            await RegistrarResolucion(db, tx, orden, id, avance, null, "El avance supera el 60 %.", actor, ct);
        await tx.CommitAsync(ct);
        return id;
    }

    /// <summary>Resuelve con el avance y consumo actuales bajo el mismo bloqueo que usa el mecánico.</summary>
    public async Task<bool> ResolverCancelacion(ResolverCancelacionCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        ulong? ordenId = await db.ExecuteScalarAsync<ulong?>(new CommandDefinition("""
            SELECT orden_id FROM solicitudes_cancelacion WHERE id = @SolicitudId
            """, solicitud, tx, cancellationToken: ct));
        if (!ordenId.HasValue) return false;
        var orden = await BloquearOrden(db, tx, ordenId.Value, ct);
        if (orden is null || orden.Pagado is null) return false;
        var estado = await db.ExecuteScalarAsync<string>(new CommandDefinition("""
            SELECT estado FROM solicitudes_cancelacion WHERE id = @SolicitudId
            """, solicitud, tx, cancellationToken: ct));
        if (estado != "SOLICITADA") return false;
        decimal avance = Avance(await Tareas(db, tx, orden.PresupuestoId, ct));
        string? motivo = solicitud.Aprobar ? null : solicitud.MotivoRechazo?.Trim();
        if (!orden.Abierta) motivo = "La reparación ya fue cerrada.";
        else if (!PoliticaCancelacion.AvanceCancelable(avance)) motivo = "El avance supera el 60 %.";

        LiquidacionCancelacion? liquidacion = null;
        if (motivo is null)
        {
            var piezas = await Piezas(db, tx, orden.PresupuestoId, ct);
            var usadas = piezas.Sum(p => p.Utilizadas * p.PrecioUnitario);
            liquidacion = PoliticaCancelacion.Liquidar(orden.Pagado.Value, orden.ManoObra, avance, usadas);
        }
        await RegistrarResolucion(db, tx, orden, solicitud.SolicitudId, avance, liquidacion, motivo, actor, ct);
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>Liquida o rechaza la solicitud, preserva las piezas sin usar y publica el resultado.</summary>
    private static async Task RegistrarResolucion(MySqlConnection db, MySqlTransaction tx, OrdenBloqueada orden,
        ulong id, decimal avance, LiquidacionCancelacion? liquidacion, string? motivo, ActorTaller actor,
        CancellationToken ct)
    {
        string estado = liquidacion is null ? "RECHAZADA" : "APROBADA";
        await db.ExecuteAsync(new CommandDefinition("""
            UPDATE solicitudes_cancelacion SET estado = @estado, progreso_al_resolver = @avance,
                costo_mano_obra_realizada = @manoObra, costo_refacciones_usadas = @piezas,
                monto_reembolsado = @reembolso, motivo_rechazo = @motivo, resuelta_en = UTC_TIMESTAMP(3)
            WHERE id = @id AND estado = 'SOLICITADA'
            """, new { id, avance, estado, manoObra = liquidacion?.ManoObraEjecutada,
                piezas = liquidacion?.PiezasUsadas, reembolso = liquidacion?.Reembolso, motivo }, tx,
            cancellationToken: ct));
        if (liquidacion is not null)
            await db.ExecuteAsync(new CommandDefinition("""
                UPDATE ordenes_servicio o JOIN atenciones a ON a.id = @AtencionId
                SET o.estado = 'CANCELADA', o.version = o.version + 1, a.estado = 'CERRADA' WHERE o.id = @Id
                """, new { orden.Id, orden.AtencionId }, tx, cancellationToken: ct));
        string mensaje = liquidacion is null
            ? "Tu cancelación fue rechazada. Consulta el motivo en la solicitud."
            : "Tu cancelación fue aprobada. Consulta la liquidación en la solicitud.";
        await NotificarOrden(db, tx, orden, "CANCELACION_RESUELTA", mensaje, ct);
        await Auditar(db, tx, actor, "solicitudes_cancelacion", id, estado, ct,
            despues: new { avance, liquidacion, motivo });
    }

    /// <summary>Asienta una devolución manual única por el importe autorizado.</summary>
    public async Task<bool> RegistrarReembolso(RegistrarReembolsoCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        var filas = await db.ExecuteAsync(new CommandDefinition("""
            UPDATE solicitudes_cancelacion SET reembolsada_en = UTC_TIMESTAMP(3)
            WHERE id = @SolicitudId AND estado = 'APROBADA' AND monto_reembolsado = @Importe AND reembolsada_en IS NULL
            """, solicitud, tx, cancellationToken: ct));
        if (filas != 1) return false;
        await Auditar(db, tx, actor, "solicitudes_cancelacion", solicitud.SolicitudId, "REEMBOLSO_ENTREGADO", ct,
            despues: new { solicitud.Importe, solicitud.Referencia });
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>Devuelve la resolución autorizada con las piezas selladas pendientes de retorno.</summary>
    public async Task<CancelacionVista?> ConsultarCancelacion(ulong id, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        var vista = await db.QuerySingleOrDefaultAsync<CancelacionVista>(new CommandDefinition("""
            SELECT s.id AS Id, s.orden_id AS OrdenId, s.estado AS Estado, s.monto_reembolsado AS Reembolso,
                s.motivo_rechazo AS MotivoRechazo, s.progreso_al_resolver AS Avance,
                s.costo_mano_obra_realizada AS ManoObraRealizada, s.costo_refacciones_usadas AS PiezasUsadas,
                s.reembolsada_en AS ReembolsadaEn
            FROM solicitudes_cancelacion s JOIN clientes c ON c.id = s.cliente_id
            WHERE s.id = @id AND (@Rol IN ('GERENTE','EMPLEADO') OR c.usuario_id = @UsuarioId)
            """, new { id, actor.Rol, actor.UsuarioId }, tx, cancellationToken: ct));
        if (vista?.Estado != "APROBADA") return vista;
        ulong presupuestoId = await db.ExecuteScalarAsync<ulong>(new CommandDefinition(
            "SELECT presupuesto_id FROM ordenes_servicio WHERE id = @OrdenId", new { vista.OrdenId },
            tx, cancellationToken: ct));
        var pendientes = (await Piezas(db, tx, presupuestoId, ct))
            .Where(p => p.Recibidas > p.Utilizadas + p.Devueltas).ToArray();
        return vista with { PiezasPorDevolver = pendientes };
    }

    /// <summary>Agrega un próximo mantenimiento asociado al expediente de una motocicleta existente.</summary>
    public async Task<ulong> ProgramarMantenimiento(ProgramarMantenimientoCommand c,ActorTaller actor,CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO mantenimientos_programados
              (motocicleta_id,orden_origen_id,descripcion,fecha_estimada,kilometraje_objetivo)
            SELECT m.id,@OrdenOrigenId,@Descripcion,@FechaEstimada,@KilometrajeObjetivo
            FROM motocicletas m WHERE m.id=@MotocicletaId
            AND (@OrdenOrigenId IS NULL OR EXISTS
               (SELECT 1 FROM ordenes_servicio o JOIN presupuestos p ON p.id=o.presupuesto_id
                JOIN atenciones a ON a.id=p.atencion_id
                WHERE o.id=@OrdenOrigenId AND a.motocicleta_id=m.id))
            """,new {c.MotocicletaId,c.OrdenOrigenId,c.Descripcion,
                c.FechaEstimada,c.KilometrajeObjetivo},tx,cancellationToken:ct));
        if(n!=1) throw new InvalidOperationException("Motocicleta u orden de origen inválida.");
        var id=await ConsultasMySql.IdNuevoAsync(db,tx,ct);
        await Auditar(db,tx,actor,"mantenimientos_programados",id,"PROGRAMAR",ct);
        await tx.CommitAsync(ct);
        return id;
    }

    /// <summary>Enumera mantenimientos visibles para el usuario actual.</summary>
    public async Task<IReadOnlyList<MantenimientoVista>> Mantenimientos(ulong motoId,ActorTaller actor,CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        var rows=await db.QueryAsync<MantenimientoVista>(new CommandDefinition("""
            SELECT m.id AS Id,m.motocicleta_id AS MotocicletaId,m.descripcion AS Descripcion,
                m.fecha_estimada AS FechaEstimada,m.kilometraje_objetivo AS KilometrajeObjetivo,
                m.estado AS Estado
            FROM mantenimientos_programados m JOIN motocicletas moto ON moto.id=m.motocicleta_id
            JOIN clientes c ON c.id=moto.cliente_id
            WHERE moto.id=@motoId AND (@Rol<>'CLIENTE' OR c.usuario_id=@UsuarioId)
            ORDER BY m.fecha_estimada,m.id
            """,new {motoId,actor.Rol,actor.UsuarioId},cancellationToken:ct));
        return rows.ToArray();
    }

    /// <summary>Devuelve la bandeja local de notificaciones; no utiliza correo externo.</summary>
    public async Task<IReadOnlyList<NotificacionVista>> Notificaciones(ActorTaller actor,CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        var rows=await db.QueryAsync<NotificacionVista>(new CommandDefinition("""
            SELECT n.id AS Id,n.tipo AS Tipo,n.mensaje AS Mensaje,
                n.creada_en AS CreadaEn,n.leida_en AS LeidaEn
            FROM notificaciones n JOIN clientes c ON c.id=n.cliente_id
            WHERE c.usuario_id=@UsuarioId ORDER BY n.creada_en DESC LIMIT 100
            """,new {actor.UsuarioId},cancellationToken:ct));
        return rows.ToArray();
    }

    /// <summary>Marca como leída solo una notificación perteneciente al usuario autenticado.</summary>
    public async Task<bool> MarcarNotificacion(ulong id,ActorTaller actor,CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            UPDATE notificaciones n JOIN clientes c ON c.id=n.cliente_id
            SET n.leida_en=COALESCE(n.leida_en,UTC_TIMESTAMP(3))
            WHERE n.id=@id AND c.usuario_id=@UsuarioId
            """,new {id,actor.UsuarioId},cancellationToken:ct));
        return n>0;
    }

    /// <summary>Obtiene términos y fechas de garantía sin revelar servicios ajenos.</summary>
    public async Task<GarantiaVista?> ConsultarGarantia(ulong ordenId,ActorTaller actor,CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        var garantia = await db.QuerySingleOrDefaultAsync<GarantiaVista>(new CommandDefinition("""
            SELECT g.id AS Id,g.orden_id AS OrdenId,g.aplica AS Aplica,
                g.duracion_dias AS DuracionDias,g.condiciones AS Condiciones,
                g.inicia_en AS IniciaEn,g.vence_en AS VenceEn,g.referencia_comprobante AS ReferenciaComprobante,
                o.estado AS EstadoOrden
            FROM garantias_servicio g JOIN ordenes_servicio o ON o.id=g.orden_id
            WHERE o.id=@ordenId AND
            """ + " " + PropiedadOrden, new { ordenId, actor.Rol, actor.UsuarioId }, cancellationToken: ct));
        return garantia is null ? null : garantia with { Estado = VigenciaGarantia.Estado(garantia.Aplica,
            garantia.EstadoOrden, garantia.IniciaEn, garantia.VenceEn, reloj.GetUtcNow().UtcDateTime) };
    }
}
