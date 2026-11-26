namespace FollowMotors.Infrastructure.Operaciones;

using System.Data;
using Dapper;
using FollowMotors.Application.Operaciones;
using FollowMotors.Infrastructure.Database;

/// <summary>Proyecta historial y reportes desde las tablas que registran las operaciones.</summary>
public sealed class RepositorioSeguimiento(ConexionMySql baseDatos) : IRepositorioSeguimiento
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<EntradaHistorial>> HistorialAsync(ulong id, int pagina, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        if (!await db.ExecuteScalarAsync<bool>(new CommandDefinition("""
                SELECT EXISTS(SELECT 1 FROM motocicletas m JOIN clientes c ON c.id = m.cliente_id
                    WHERE m.id = @id AND (@Rol IN ('GERENTE','EMPLEADO') OR c.usuario_id = @UsuarioId))
                """, new { id, actor.Rol, actor.UsuarioId }, tx, cancellationToken: ct)))
            throw new KeyNotFoundException("No existe la motocicleta.");

        var filas = await db.QueryAsync<EntradaHistorial>(new CommandDefinition("""
            SELECT a.id AS AtencionId, a.estado AS EstadoAtencion, a.kilometraje_ingreso AS KilometrajeIngreso,
                a.mecanico_id AS MecanicoId, a.motivo AS Motivo, a.diagnostico AS Diagnostico, a.ingresada_en AS Ingreso,
                a.diagnosticada_en AS DiagnosticadaEn, r.importe_cobrado AS TarifaCobrada, r.cobrado_en AS CobradoEn,
                o.id AS OrdenId, o.estado AS EstadoOrden, ar.total_pagado AS Anticipo,
                o.terminada_en AS TerminadaEn, o.entregada_en AS EntregadaEn
            FROM atenciones a JOIN clientes c ON c.id = a.cliente_id
            LEFT JOIN cobros_revision r ON r.atencion_id = a.id
            LEFT JOIN presupuestos p ON p.atencion_id = a.id
            LEFT JOIN ordenes_servicio o ON o.presupuesto_id = p.id
            LEFT JOIN anticipos_reparacion ar ON ar.orden_id = o.id
            WHERE a.motocicleta_id = @id AND (@Rol IN ('GERENTE','EMPLEADO') OR c.usuario_id = @UsuarioId)
            ORDER BY a.ingresada_en DESC, a.id DESC LIMIT 50 OFFSET @offset
            """, new { id, actor.Rol, actor.UsuarioId, offset = (pagina - 1) * 50 }, tx, cancellationToken: ct));
        return filas.AsList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IndicadorOperativo>> ReporteAsync(DateTime desde, DateTime hasta, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        var filas = await db.QueryAsync<IndicadorOperativo>(new CommandDefinition("""
            SELECT 'CITAS' AS Modulo, c.estado AS Estado, COUNT(*) AS Total
            FROM citas c JOIN franjas_citas f ON f.id = c.franja_id WHERE f.inicio >= @desde AND f.inicio < @hasta
            GROUP BY c.estado
            UNION ALL
            SELECT 'ORDENES', estado, COUNT(*) FROM ordenes_servicio WHERE creada_en >= @desde AND creada_en < @hasta
            GROUP BY estado
            UNION ALL
            SELECT 'CANCELACIONES', estado, COUNT(*) FROM solicitudes_cancelacion
            WHERE solicitada_en >= @desde AND solicitada_en < @hasta GROUP BY estado
            UNION ALL
            SELECT 'MANTENIMIENTOS', estado, COUNT(*) FROM mantenimientos_programados
            WHERE fecha_estimada >= @desde AND fecha_estimada < @hasta GROUP BY estado
            ORDER BY Modulo, Estado
            """, new { desde, hasta }, cancellationToken: ct));
        return filas.AsList();
    }

    /// <inheritdoc />
    public async Task ResolverMantenimientoAsync(ResolverMantenimientoCommand solicitud, ulong usuarioId, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        string estado = solicitud.Realizado ? "REALIZADO" : "CANCELADO";
        int filas = await db.ExecuteAsync(new CommandDefinition("""
            UPDATE mantenimientos_programados m SET m.estado = @estado
            WHERE m.id = @Id AND m.estado = 'PENDIENTE' AND
                (NOT @Realizado OR EXISTS(
                    SELECT 1 FROM ordenes_servicio o JOIN presupuestos p ON p.id = o.presupuesto_id
                    JOIN atenciones a ON a.id = p.atencion_id
                    WHERE o.id = @OrdenId AND o.estado = 'ENTREGADA' AND a.motocicleta_id = m.motocicleta_id
                ))
            """, new { solicitud.Id, solicitud.Realizado, solicitud.OrdenId, estado }, tx, cancellationToken: ct));
        if (filas != 1) throw new InvalidOperationException("El mantenimiento debe estar pendiente y tener una entrega válida al completarlo.");
        await AuditoriaSql.RegistrarAsync(db, tx, usuarioId, "mantenimientos_programados", solicitud.Id, estado,
            null, System.Text.Json.JsonSerializer.Serialize(new { solicitud.OrdenId }), ct);
        await tx.CommitAsync(ct);
    }
}
