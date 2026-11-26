namespace FollowMotors.Infrastructure.Operaciones;

using Dapper;
using FollowMotors.Application.Operaciones;
using FollowMotors.Domain.Rules.Operaciones;

/// <summary>Solicitudes de cancelación, garantías, mantenimientos y bandeja local de notificaciones.</summary>
public sealed partial class OperacionesTallerMySql
{
    /// <summary>Solicita cancelación una sola vez, comprobando propiedad de la orden.</summary>
    public async Task<ulong> SolicitarCancelacion(ulong ordenId,ActorTaller actor,CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO solicitudes_cancelacion(orden_id,cliente_id)
            SELECT o.id,a.cliente_id FROM ordenes_servicio o
            JOIN presupuestos p ON p.id=o.presupuesto_id JOIN atenciones a ON a.id=p.atencion_id
            JOIN clientes c ON c.id=a.cliente_id
            WHERE o.id=@ordenId AND o.estado IN('PENDIENTE_PIEZAS','LISTA','EN_EJECUCION')
              AND (@Rol<>'CLIENTE' OR c.usuario_id=@UsuarioId)
            """,new {ordenId,actor.Rol,actor.UsuarioId},tx,cancellationToken:ct));
        if(n!=1) throw new InvalidOperationException("La orden no admite cancelación o no pertenece al solicitante.");
        var id=await IdNuevo(db,tx,ct);
        await Auditar(db,tx,actor,"solicitudes_cancelacion",id,"SOLICITAR",ct);
        await tx.CommitAsync(ct);
        return id;
    }

    private sealed class DatosCancelacion
    {
        public ulong OrdenId {get;set;}
        public decimal ManoObra {get;set;}
        public decimal Abonado {get;set;}
        public decimal Avance {get;set;}
        public decimal PiezasUtilizadas {get;set;}
    }

    /// <summary>Congela la orden y calcula el reintegro con el progreso y los materiales realmente usados.</summary>
    public async Task<bool> ResolverCancelacion(ResolverCancelacionCommand c,ActorTaller actor,CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var datos=await db.QuerySingleOrDefaultAsync<DatosCancelacion>(new CommandDefinition("""
            SELECT s.orden_id AS OrdenId,p.mano_obra_acordada AS ManoObra,
            COALESCE(ap.total_pagado,0) AS Abonado,
            COALESCE((SELECT SUM(t.peso_porcentual*t.avance_tarea/100) FROM tareas_servicio t
                WHERE t.presupuesto_id=p.id),0) AS Avance,
            COALESCE((SELECT SUM(r.cantidad_utilizada*r.precio_unitario_cliente)
                FROM refacciones_servicio r WHERE r.presupuesto_id=p.id),0) AS PiezasUtilizadas
            FROM solicitudes_cancelacion s JOIN ordenes_servicio o ON o.id=s.orden_id
            JOIN presupuestos p ON p.id=o.presupuesto_id
            LEFT JOIN anticipos_reparacion ap ON ap.orden_id=o.id
            WHERE s.id=@SolicitudId AND s.estado='SOLICITADA'
            AND o.estado IN('PENDIENTE_PIEZAS','LISTA','EN_EJECUCION')
            FOR UPDATE
            """,new {c.SolicitudId},tx,cancellationToken:ct));
        if(datos is null) return false;
        var manoObraEjecutada=Math.Round(datos.ManoObra*datos.Avance/100m,2,MidpointRounding.AwayFromZero);
        var reintegro=ReglasOperacionesTaller.CalcularReembolso(datos.Abonado,manoObraEjecutada,datos.PiezasUtilizadas);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            UPDATE solicitudes_cancelacion SET estado=@Estado,
                progreso_al_resolver=@Avance,costo_mano_obra_realizada=@ManoObraEjecutada,
                costo_refacciones_usadas=@PiezasUtilizadas,
                monto_reembolsado=@Reintegro,motivo_rechazo=@MotivoRechazo,
                resuelta_en=UTC_TIMESTAMP(3)
            WHERE id=@SolicitudId AND estado='SOLICITADA'
            """,new {c.SolicitudId,Estado=c.Aprobar?"APROBADA":"RECHAZADA",
                datos.Avance,ManoObraEjecutada=manoObraEjecutada,datos.PiezasUtilizadas,
                Reintegro=c.Aprobar?reintegro:(decimal?)null,
                MotivoRechazo=c.Aprobar?null:c.MotivoRechazo},tx,cancellationToken:ct));
        if(n!=1) return false;
        if(c.Aprobar)
        {
            await db.ExecuteAsync(new CommandDefinition("""
                UPDATE ordenes_servicio SET estado='CANCELADA',version=version+1
                WHERE id=@OrdenId
                """,new {datos.OrdenId},tx,cancellationToken:ct));
            await db.ExecuteAsync(new CommandDefinition("""
                UPDATE atenciones a JOIN presupuestos p ON p.atencion_id=a.id
                JOIN ordenes_servicio o ON o.presupuesto_id=p.id SET a.estado='CERRADA'
                WHERE o.id=@OrdenId
                """,new {datos.OrdenId},tx,cancellationToken:ct));
        }
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT IGNORE INTO notificaciones(cliente_id,orden_id,tipo,mensaje)
            SELECT s.cliente_id,s.orden_id,'CANCELACION_RESUELTA',
                CASE WHEN @Aprobar=1 THEN 'Tu cancelación fue aprobada; consulta el reintegro pendiente.'
                     ELSE 'Tu cancelación fue rechazada; consulta el motivo.' END
            FROM solicitudes_cancelacion s WHERE s.id=@SolicitudId
            """,new {c.SolicitudId,c.Aprobar},tx,cancellationToken:ct));
        await Auditar(db,tx,actor,"solicitudes_cancelacion",c.SolicitudId,
            c.Aprobar?"APROBAR":"RECHAZAR",ct);
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>Filtra las cancelaciones por la cuenta titular del expediente.</summary>
    public async Task<CancelacionVista?> ConsultarCancelacion(ulong id,ActorTaller actor,CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        return await db.QuerySingleOrDefaultAsync<CancelacionVista>(new CommandDefinition("""
            SELECT s.id AS Id,s.orden_id AS OrdenId,s.estado AS Estado,
                   s.monto_reembolsado AS Reembolso,s.motivo_rechazo AS MotivoRechazo
            FROM solicitudes_cancelacion s JOIN clientes c ON c.id=s.cliente_id
            WHERE s.id=@id AND (@Rol<>'CLIENTE' OR c.usuario_id=@UsuarioId)
            """,new {id,actor.Rol,actor.UsuarioId},cancellationToken:ct));
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
        var id=await IdNuevo(db,tx,ct);
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
        return await db.QuerySingleOrDefaultAsync<GarantiaVista>(new CommandDefinition("""
            SELECT g.id AS Id,g.orden_id AS OrdenId,g.aplica AS Aplica,
                g.duracion_dias AS DuracionDias,g.condiciones AS Condiciones,
                g.inicia_en AS IniciaEn,g.vence_en AS VenceEn
            FROM garantias_servicio g JOIN ordenes_servicio o ON o.id=g.orden_id
            WHERE o.id=@ordenId AND
            """+PropiedadOrden,new {ordenId,actor.Rol,actor.UsuarioId},cancellationToken:ct));
    }
}
