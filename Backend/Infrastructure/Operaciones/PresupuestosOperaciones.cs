namespace FollowMotors.Infrastructure.Operaciones;

using System.Data;
using Dapper;
using FollowMotors.Application.Operaciones;
using FollowMotors.Domain.Rules.Servicios;
using FollowMotors.Infrastructure.Database;
using MySqlConnector;

/// <summary>Persiste el presupuesto, el acuerdo presencial y el anticipo junto a sus comprobantes.</summary>
public sealed partial class OperacionesTallerMySql
{
    /// <summary>Guarda un presupuesto y su plan tras verificar que la revisión fue pagada.</summary>
    public async Task<ulong> CrearPresupuesto(CrearPresupuestoCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var atencion = await db.QuerySingleOrDefaultAsync<(string Estado, ulong UsuarioMecanico)>(new CommandDefinition("""
            SELECT a.estado, e.usuario_id FROM atenciones a JOIN empleados e ON e.id = a.mecanico_id
            WHERE a.id = @AtencionId AND EXISTS(SELECT 1 FROM cobros_revision c WHERE c.atencion_id = a.id)
            FOR UPDATE OF a
            """, solicitud, tx, cancellationToken: ct));
        if (atencion.Estado != "DIAGNOSTICADA")
            throw new InvalidOperationException("La atención requiere diagnóstico y cobro previos.");
        if (actor.Rol != "GERENTE" && atencion.UsuarioMecanico != actor.UsuarioId)
            throw new UnauthorizedAccessException("El mecánico asignado o gerencia prepara la cotización.");
        int filas = await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO presupuestos (atencion_id,tipo_servicio_id,descripcion_trabajo,mano_obra_acordada,
                refacciones_acordadas,entrega_estimada,registrado_por_usuario_id)
            SELECT @AtencionId,t.id,@descripcion,@ManoObra,@Refacciones,@fecha,@UsuarioId
            FROM tipos_servicio t WHERE t.id = @TipoServicioId AND t.activo
            """, new { solicitud.AtencionId, solicitud.TipoServicioId, descripcion = solicitud.Descripcion.Trim(),
                solicitud.ManoObra, solicitud.Refacciones, fecha = solicitud.EntregaEstimada?.UtcDateTime, actor.UsuarioId },
            tx, cancellationToken: ct));
        if (filas != 1) throw new KeyNotFoundException("El tipo de servicio no está disponible.");
        ulong id = await ConsultasMySql.IdNuevoAsync(db, tx, ct);
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO tareas_servicio (presupuesto_id,descripcion,orden,peso_porcentual)
            VALUES (@id,@descripcion,@orden,@Peso)
            """, solicitud.Tareas.Select((tarea, indice) => new
            { id, descripcion = tarea.Descripcion.Trim(), orden = indice + 1, tarea.Peso }), tx, cancellationToken: ct));
        await Auditar(db, tx, actor, "presupuestos", id, "CREAR", ct);
        await tx.CommitAsync(ct);
        return id;
    }

    /// <summary>Confirma el desglose y fija la decisión que el personal documentó en sucursal.</summary>
    public async Task<bool> DecidirPresupuesto(DecidirPresupuestoCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var presupuesto = await BloquearPresupuesto(db, tx, solicitud.PresupuestoId, ct);
        if (presupuesto.Estado != "BORRADOR") return false;
        if (solicitud.Aceptar)
        {
            var tareas = await Tareas(db, tx, presupuesto.Id, ct);
            var piezas = await Piezas(db, tx, presupuesto.Id, ct);
            if (!CalculadoraAvance.PlanValido(tareas.Select(x => x.Peso))
                || piezas.Sum(pieza => pieza.Solicitadas * pieza.PrecioUnitario) != presupuesto.Refacciones)
                throw new InvalidOperationException("Los pesos y el desglose de piezas deben coincidir con el presupuesto.");
        }

        var estado = solicitud.Aceptar ? "ACEPTADO" : "RECHAZADO";
        int modificados = await db.ExecuteAsync(new CommandDefinition("""
            UPDATE presupuestos p JOIN atenciones a ON a.id = p.atencion_id
            JOIN cobros_revision cr ON cr.atencion_id = a.id
            SET p.estado = @estado, p.acordado_en = UTC_TIMESTAMP(3),
                a.estado = IF(@Aceptar, a.estado, 'SIN_REPARACION')
            WHERE p.id = @Id AND a.estado = 'DIAGNOSTICADA'
            """, new { presupuesto.Id, estado, solicitud.Aceptar }, tx, cancellationToken: ct));
        if (modificados == 0) return false;
        await Auditar(db, tx, actor, "presupuestos", presupuesto.Id, estado, ct);
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>Obtiene los detalles del presupuesto visible para la sesión.</summary>
    public async Task<PresupuestoVista?> ConsultarPresupuesto(ulong id, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        var vista = await db.QuerySingleOrDefaultAsync<PresupuestoVista>(new CommandDefinition("""
            SELECT p.id AS Id, p.atencion_id AS AtencionId, p.descripcion_trabajo AS DescripcionTrabajo,
                p.mano_obra_acordada AS ManoObra, p.refacciones_acordadas AS Refacciones,
                p.total_acordado AS Total, p.estado AS Estado, p.entrega_estimada AS EntregaEstimada
            FROM presupuestos p WHERE p.id = @id AND (@Rol <> 'CLIENTE' OR p.estado <> 'BORRADOR') AND
            """ + " " + PropiedadPresupuesto, new { id, actor.Rol, actor.UsuarioId }, tx, cancellationToken: ct));
        if (vista is null) return null;
        return vista with { Tareas = await Tareas(db, tx, id, ct), Piezas = await Piezas(db, tx, id, ct) };
    }

    /// <summary>Guarda la orden, el pago completo y la garantía informada como un solo cambio.</summary>
    public async Task<ulong> Contratar(ContratarServicioCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var presupuesto = await BloquearPresupuesto(db, tx, solicitud.PresupuestoId, ct);
        if (presupuesto.Estado != "ACEPTADO")
            throw new InvalidOperationException("El presupuesto debe estar aceptado en sucursal.");
        var estado =
            (await Piezas(db, tx, presupuesto.Id, ct)).Any(p => p.Recibidas < p.Solicitadas)
            ? "PENDIENTE_PIEZAS" : "LISTA";
        int filas = await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO ordenes_servicio (presupuesto_id,estado)
            SELECT p.id,@estado FROM presupuestos p JOIN atenciones a ON a.id = p.atencion_id
            JOIN cobros_revision cr ON cr.atencion_id = a.id WHERE p.id = @Id AND a.estado = 'DIAGNOSTICADA'
            """, new { presupuesto.Id, estado }, tx, cancellationToken: ct));
        if (filas != 1) throw new InvalidOperationException("El presupuesto ya se contrató o la revisión no es válida.");
        ulong ordenId = await ConsultasMySql.IdNuevoAsync(db, tx, ct);
        await GuardarAnticipo(db, tx, ordenId, presupuesto, solicitud.ManoObra, solicitud.Refacciones,
            solicitud.Comprobante.Trim(), actor, ct);
        await db.ExecuteAsync(new CommandDefinition("""
            UPDATE atenciones a JOIN presupuestos p ON p.atencion_id = a.id
            SET a.estado = 'CONTRATADA' WHERE p.id = @Id
            """, new { presupuesto.Id }, tx, cancellationToken: ct));
        await Auditar(db, tx, actor, "ordenes_servicio", ordenId, "CONTRATAR", ct);
        await tx.CommitAsync(ct);
        return ordenId;
    }

    /// <summary>Registra el pago de una orden previa pendiente de anticipo, con las mismas reglas.</summary>
    public async Task<ulong> RegistrarAnticipo(RegistrarAnticipoCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var orden = await BloquearOrden(db, tx, solicitud.OrdenId, ct)
            ?? throw new KeyNotFoundException("No existe la orden.");
        if (!orden.Abierta || orden.Pagado.HasValue)
            throw new InvalidOperationException("La orden no está esperando anticipo.");
        var presupuesto = await BloquearPresupuesto(db, tx, orden.PresupuestoId, ct);
        if (presupuesto.Estado != "ACEPTADO")
            throw new InvalidOperationException("El presupuesto debe estar aceptado en sucursal.");
        ulong id = await GuardarAnticipo(db, tx, orden.Id, presupuesto, solicitud.ManoObra, solicitud.Refacciones,
            solicitud.Comprobante!.Trim(), actor, ct);
        await tx.CommitAsync(ct);
        return id;
    }

    /// <summary>Persiste el importe exacto acordado y la constancia de garantía.</summary>
    private static async Task<ulong> GuardarAnticipo(MySqlConnection db, MySqlTransaction tx, ulong ordenId,
        PresupuestoBloqueado presupuesto, decimal manoObra, decimal refacciones, string comprobante,
        ActorTaller actor, CancellationToken ct)
    {
        if (manoObra != presupuesto.ManoObra || refacciones != presupuesto.Refacciones)
            throw new InvalidOperationException("El pago debe coincidir con los importes acordados de mano de obra y piezas.");
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO anticipos_reparacion (orden_id,monto_mano_obra,monto_refacciones,
                referencia_comprobante,registrado_por_usuario_id)
            VALUES (@ordenId,@manoObra,@refacciones,@comprobante,@UsuarioId)
            """, new { ordenId, manoObra, refacciones, comprobante, actor.UsuarioId }, tx, cancellationToken: ct));
        ulong pago = await ConsultasMySql.IdNuevoAsync(db, tx, ct);
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO garantias_servicio (orden_id,aplica,duracion_dias,condiciones,informada_en,referencia_comprobante)
            SELECT @ordenId, t.garantia_dias > 0, t.garantia_dias,
                IF(t.garantia_dias = 0,'Sin cobertura del taller',COALESCE(NULLIF(t.cobertura,''),'Consultar condiciones del servicio')),
                UTC_TIMESTAMP(3), @comprobante
            FROM presupuestos p JOIN tipos_servicio t ON t.id = p.tipo_servicio_id WHERE p.id = @Id
            ON DUPLICATE KEY UPDATE garantias_servicio.informada_en = UTC_TIMESTAMP(3),
                garantias_servicio.referencia_comprobante = @comprobante
            """, new { ordenId, presupuesto.Id, comprobante }, tx, cancellationToken: ct));
        await Auditar(db, tx, actor, "anticipos_reparacion", pago, "COBRAR_ANTICIPO", ct,
            despues: new { ordenId, manoObra, refacciones, comprobante });
        return pago;
    }

    /// <summary>Registra las condiciones de cobertura declaradas por gerencia.</summary>
    public async Task<ulong> RegistrarTipoServicio(RegistrarTipoServicioCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO tipos_servicio (nombre,descripcion,garantia_dias,cobertura)
            VALUES (@nombre,@descripcion,@GarantiaDias,@cobertura)
            """, new { nombre = solicitud.Nombre.Trim(), descripcion = solicitud.Descripcion?.Trim(),
                solicitud.GarantiaDias, cobertura = solicitud.Cobertura?.Trim() }, tx, cancellationToken: ct));
        ulong id = await ConsultasMySql.IdNuevoAsync(db, tx, ct);
        await Auditar(db, tx, actor, "tipos_servicio", id, "CREAR", ct);
        await tx.CommitAsync(ct);
        return id;
    }

    /// <summary>Lista servicios activos para preparar presupuestos.</summary>
    public async Task<IReadOnlyList<TipoServicioVista>> TiposServicio(CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        return (await db.QueryAsync<TipoServicioVista>(new CommandDefinition("""
            SELECT id AS Id, nombre AS Nombre, descripcion AS Descripcion, garantia_dias AS GarantiaDias,
                cobertura AS Cobertura FROM tipos_servicio WHERE activo ORDER BY nombre
            """, cancellationToken: ct))).AsList();
    }

    /// <summary>Congela la cotización para verificar sus piezas y evitar dos decisiones simultáneas.</summary>
    private static async Task<PresupuestoBloqueado> BloquearPresupuesto(MySqlConnection db, MySqlTransaction tx,
        ulong id, CancellationToken ct) =>
        await db.QuerySingleOrDefaultAsync<PresupuestoBloqueado>(new CommandDefinition("""
            SELECT id AS Id, estado AS Estado, mano_obra_acordada AS ManoObra,
                refacciones_acordadas AS Refacciones FROM presupuestos WHERE id = @id FOR UPDATE
            """, new { id }, tx, cancellationToken: ct))
        ?? throw new KeyNotFoundException("No existe el presupuesto.");

    /// <summary>Importes y estado que se conservan bajo el bloqueo de la cotización.</summary>
    private sealed record PresupuestoBloqueado(ulong Id, string Estado, decimal ManoObra, decimal Refacciones);
}
