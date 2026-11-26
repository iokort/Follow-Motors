namespace FollowMotors.Infrastructure.Operaciones;

using System.Data;
using Dapper;
using static FollowMotors.Infrastructure.Operaciones.TallerSql;
using FollowMotors.Application.Operaciones;
using FollowMotors.Domain.Rules.Servicios;
using FollowMotors.Infrastructure.Database;

/// <summary>Aplica cambios a órdenes y tareas mientras conserva el bloqueo de la orden correspondiente.</summary>
public sealed class RepositorioEjecucion(ConexionMySql baseDatos) : IEjecucionTaller
{
    /// <summary>Cambia el responsable por gerencia, usando la misma exclusión que la asignación inicial.</summary>
    public async Task<bool> AsignarMecanico(AsignarMecanicoCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var bloqueo = await BloqueoMySql.AdquirirAsync(db, "mecanicos", ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var orden = await BloquearOrden(db, tx, solicitud.OrdenId, ct);
        if (orden is null || !orden.Abierta) return false;
        int filas = await db.ExecuteAsync(new CommandDefinition("""
            UPDATE atenciones a JOIN empleados e ON e.id = @MecanicoId AND e.es_mecanico AND e.disponible
            JOIN usuarios u ON u.id = e.usuario_id AND u.activo
            SET a.mecanico_id = e.id WHERE a.id = @AtencionId
            """, new { orden.AtencionId, solicitud.MecanicoId }, tx, cancellationToken: ct));
        if (filas != 1) return false;
        await Auditar(db, tx, actor, "ordenes_servicio", orden.Id, "ASIGNAR_MECANICO", ct,
            despues: new { solicitud.MecanicoId });
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>Registra progreso y emite el aviso de 25 % una sola vez por reparación.</summary>
    public async Task<bool> AvanzarTarea(ActualizarTareaCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var orden = await BloquearOrden(db, tx, solicitud.OrdenId, ct);
        if (orden is null || !orden.PuedeEjecutarse) return false;
        ExigirResponsable(orden, actor);
        var tareas = await Tareas(db, tx, orden.PresupuestoId, ct);
        var tarea = tareas.FirstOrDefault(t => t.Id == solicitud.TareaId);
        if (tarea is null || solicitud.Porcentaje < tarea.Porcentaje) return false;
        if (solicitud.Porcentaje == tarea.Porcentaje) return true;

        var avanceActual = Avance(tareas.Select(t => t.Id == tarea.Id ? t with { Porcentaje = solicitud.Porcentaje } : t));
        string estado = solicitud.Porcentaje == 100m ? "COMPLETADA" : "EN_PROCESO";
        await db.ExecuteAsync(new CommandDefinition("""
            UPDATE tareas_servicio SET avance_tarea = @Porcentaje, evidencia = @evidencia,
                estado = @estado, actualizada_en = UTC_TIMESTAMP(3) WHERE id = @TareaId
            """, new { solicitud.TareaId, solicitud.Porcentaje, evidencia = solicitud.Evidencia?.Trim(), estado },
            tx, cancellationToken: ct));
        bool avisar = orden.AvisoAvance is null && avanceActual >= CalculadoraAvance.UmbralAviso;
        await db.ExecuteAsync(new CommandDefinition("""
            UPDATE ordenes_servicio SET estado = 'EN_EJECUCION',
                iniciada_en = COALESCE(iniciada_en,UTC_TIMESTAMP(3)), version = version + 1,
                aviso_avance_25_en = IF(@avisar,UTC_TIMESTAMP(3),aviso_avance_25_en) WHERE id = @Id
            """, new { orden.Id, avisar }, tx, cancellationToken: ct));
        if (avisar)
            await NotificarOrden(db, tx, orden, "AVANCE_25",
                "Tu servicio alcanzó al menos el 25 % de avance.", ct);
        await Auditar(db, tx, actor, "tareas_servicio", tarea.Id, "ACTUALIZAR_AVANCE", ct,
            new { tarea.Porcentaje }, new { solicitud.Porcentaje, solicitud.Evidencia, avanceActual });
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>Actualiza una estimación de fecha sin cambiar el avance calculado.</summary>
    public async Task<bool> EstimarEntrega(EstimarEntregaCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        var fecha = solicitud.EntregaEstimada.UtcDateTime;
        if (fecha.Year < 1000) throw new ArgumentException("La fecha de entrega es inválida.");
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var orden = await BloquearOrden(db, tx, solicitud.OrdenId, ct);
        if (orden is null || !orden.Abierta) return false;
        ExigirResponsable(orden, actor);
        await db.ExecuteAsync(new CommandDefinition("""
            UPDATE presupuestos SET entrega_estimada = @fecha WHERE id = @PresupuestoId
            """, new { orden.PresupuestoId, fecha }, tx, cancellationToken: ct));
        await Auditar(db, tx, actor, "ordenes_servicio", orden.Id, "ESTIMAR_ENTREGA", ct, despues: new { fecha });
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>Marca finalización técnica solo cuando todas las tareas y piezas están resueltas.</summary>
    public async Task<bool> Finalizar(ulong ordenId, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var orden = await BloquearOrden(db, tx, ordenId, ct);
        if (orden is null || !orden.PuedeEjecutarse) return false;
        ExigirResponsable(orden, actor);
        if (Avance(await Tareas(db, tx, orden.PresupuestoId, ct)) != 100m
            || (await Piezas(db, tx, orden.PresupuestoId, ct)).Any(p => p.Recibidas < p.Solicitadas))
            return false;
        await db.ExecuteAsync(new CommandDefinition("""
            UPDATE ordenes_servicio SET estado = 'TERMINADA', terminada_en = UTC_TIMESTAMP(3),
                version = version + 1 WHERE id = @ordenId
            """, new { ordenId }, tx, cancellationToken: ct));
        await NotificarOrden(db, tx, orden, "SERVICIO_TERMINADO", "Tu motocicleta está lista para entrega.", ct);
        await Auditar(db, tx, actor, "ordenes_servicio", ordenId, "FINALIZAR", ct);
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>Asienta la entrega real y habilita la vigencia de la cobertura acordada.</summary>
    public async Task<bool> Entregar(EntregarOrdenCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        if (solicitud.Comprobante?.Length > 120) throw new ArgumentException("El comprobante es demasiado largo.");
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var orden = await BloquearOrden(db, tx, solicitud.OrdenId, ct);
        if (orden is null || orden.Estado != "TERMINADA" || orden.Pagado is null) return false;
        await db.ExecuteAsync(new CommandDefinition("""
            UPDATE ordenes_servicio o JOIN atenciones a ON a.id = @AtencionId
            SET o.estado = 'ENTREGADA', o.entregada_en = UTC_TIMESTAMP(3),
                o.version = o.version + 1, a.estado = 'CERRADA' WHERE o.id = @Id
            """, new { orden.Id, orden.AtencionId }, tx, cancellationToken: ct));
        await db.ExecuteAsync(new CommandDefinition("""
            UPDATE garantias_servicio g JOIN ordenes_servicio o ON o.id = g.orden_id
            SET g.inicia_en = o.entregada_en, g.vence_en = DATE_ADD(o.entregada_en,INTERVAL g.duracion_dias DAY)
            WHERE g.orden_id = @Id AND g.inicia_en IS NULL AND g.aplica
            """, new { orden.Id }, tx, cancellationToken: ct));
        await Auditar(db, tx, actor, "ordenes_servicio", orden.Id, "ENTREGAR", ct,
            despues: new { solicitud.Comprobante });
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>Devuelve progreso real, tareas, piezas y fecha prevista de una orden autorizada.</summary>
    public async Task<OrdenVista?> ConsultarOrden(ulong id, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        var vista = await db.QuerySingleOrDefaultAsync<OrdenVista>(new CommandDefinition("""
            SELECT o.id AS Id, o.presupuesto_id AS PresupuestoId, o.estado AS Estado,
                CAST(0 AS DECIMAL(5,2)) AS Avance, o.terminada_en AS TerminadaEn,
                o.entregada_en AS EntregadaEn, p.entrega_estimada AS EntregaEstimada, a.mecanico_id AS MecanicoId
            FROM ordenes_servicio o JOIN presupuestos p ON p.id = o.presupuesto_id
            JOIN atenciones a ON a.id = p.atencion_id WHERE o.id = @id AND
            """ + " " + PropiedadOrden, new { id, actor.Rol, actor.UsuarioId }, tx, cancellationToken: ct));
        if (vista is null) return null;
        var tareas = await Tareas(db, tx, vista.PresupuestoId, ct);
        var piezas = await Piezas(db, tx, vista.PresupuestoId, ct);
        return vista with { Avance = Avance(tareas), Tareas = tareas, Piezas = piezas };
    }
}
