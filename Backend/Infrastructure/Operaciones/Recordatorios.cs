namespace FollowMotors.Infrastructure.Operaciones;

using Dapper;
using FollowMotors.Application.Operaciones;
using FollowMotors.Infrastructure.Database;

/// <summary>Publica en MySQL los avisos del canal interno y oculta los eventos que ya perdieron vigencia.</summary>
public sealed class RepositorioRecordatorios(ConexionMySql baseDatos) : IRepositorioRecordatorios
{
    /// <inheritdoc />
    public async Task<bool> PreferenciaAsync(ulong usuarioId, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        return await db.ExecuteScalarAsync<bool>(new CommandDefinition("""
            SELECT EXISTS(SELECT 1 FROM preferencias_recordatorio p JOIN clientes c ON c.id = p.cliente_id
                WHERE c.usuario_id = @usuarioId AND p.habilitado)
            """, new { usuarioId }, cancellationToken: ct));
    }

    /// <inheritdoc />
    public async Task CambiarPreferenciaAsync(ulong usuarioId, bool habilitado, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        ulong? clienteId = await db.ExecuteScalarAsync<ulong?>(new CommandDefinition(
            "SELECT id FROM clientes WHERE usuario_id = @usuarioId", new { usuarioId }, tx, cancellationToken: ct));
        if (clienteId is null) throw new KeyNotFoundException("No existe el cliente.");
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO preferencias_recordatorio (cliente_id,habilitado) VALUES (@clienteId,@habilitado)
            ON DUPLICATE KEY UPDATE habilitado = @habilitado, actualizado_en = UTC_TIMESTAMP(3)
            """, new { clienteId, habilitado }, tx, cancellationToken: ct));
        await AuditoriaSql.RegistrarAsync(db, tx, usuarioId, "preferencias_recordatorio", clienteId.Value,
            habilitado ? "ACTIVAR" : "REVOCAR", null, null, ct);
        await tx.CommitAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecordatorioVista>> ListarAsync(ulong usuarioId, DateTime ahora, DateTime hoy,
        CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        var avisos = await db.QueryAsync<RecordatorioVista>(new CommandDefinition("""
            SELECT r.id AS Id, 'CITA' AS Tipo, c.id AS EventoId, m.id AS MotocicletaId,
                CONCAT(m.marca,' ',m.modelo) AS Motocicleta, c.motivo AS Servicio,
                r.fecha_evento AS FechaEvento, r.generado_en AS GeneradoEn
            FROM recordatorios_taller r JOIN clientes t ON t.id = r.cliente_id
            JOIN preferencias_recordatorio p ON p.cliente_id = t.id AND p.habilitado
            JOIN citas c ON c.id = r.cita_id AND c.estado = 'CONFIRMADA'
            JOIN franjas_citas f ON f.id = c.franja_id AND f.habilitada AND f.inicio = r.fecha_evento
            JOIN motocicletas m ON m.id = c.motocicleta_id AND m.cliente_id = t.id
            WHERE t.usuario_id = @usuarioId AND f.inicio > @ahora
            UNION ALL
            SELECT r.id, 'MANTENIMIENTO', a.id, m.id, CONCAT(m.marca,' ',m.modelo), a.descripcion,
                r.fecha_evento, r.generado_en
            FROM recordatorios_taller r JOIN clientes t ON t.id = r.cliente_id
            JOIN preferencias_recordatorio p ON p.cliente_id = t.id AND p.habilitado
            JOIN mantenimientos_programados a ON a.id = r.mantenimiento_id AND a.estado = 'PENDIENTE'
            JOIN motocicletas m ON m.id = a.motocicleta_id AND m.cliente_id = t.id
            WHERE t.usuario_id = @usuarioId AND a.fecha_estimada = DATE(r.fecha_evento) AND a.fecha_estimada >= @hoy
            ORDER BY FechaEvento, Id LIMIT 100
            """, new { usuarioId, ahora, hoy }, cancellationToken: ct));
        return avisos.AsList();
    }

    /// <inheritdoc />
    public async Task<int> GenerarAsync(DateTime ahora, DateTime hastaCitas, DateTime hoy,
        DateTime hastaMantenimientos, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var bloqueo = await BloqueoMySql.AdquirirAsync(db, "recordatorios", ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        int citas = await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO recordatorios_taller (cliente_id,cita_id,fecha_evento)
            SELECT t.id,c.id,f.inicio FROM citas c JOIN franjas_citas f ON f.id = c.franja_id AND f.habilitada
            JOIN motocicletas m ON m.id = c.motocicleta_id AND m.cliente_id = c.cliente_id
            JOIN clientes t ON t.id = c.cliente_id JOIN usuarios u ON u.id = t.usuario_id AND u.activo
            JOIN preferencias_recordatorio p ON p.cliente_id = t.id AND p.habilitado
            LEFT JOIN recordatorios_taller r ON r.cita_id = c.id AND r.fecha_evento = f.inicio
            WHERE r.id IS NULL AND c.estado = 'CONFIRMADA' AND f.inicio > @ahora AND f.inicio <= @hastaCitas
            """, new { ahora, hastaCitas }, tx, cancellationToken: ct));
        int mantenimientos = await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO recordatorios_taller (cliente_id,mantenimiento_id,fecha_evento)
            SELECT t.id,a.id,a.fecha_estimada FROM mantenimientos_programados a
            JOIN motocicletas m ON m.id = a.motocicleta_id JOIN clientes t ON t.id = m.cliente_id
            JOIN usuarios u ON u.id = t.usuario_id AND u.activo
            JOIN preferencias_recordatorio p ON p.cliente_id = t.id AND p.habilitado
            LEFT JOIN recordatorios_taller r ON r.mantenimiento_id = a.id AND r.fecha_evento = a.fecha_estimada
            WHERE r.id IS NULL AND a.estado = 'PENDIENTE' AND a.fecha_estimada BETWEEN @hoy AND @hastaMantenimientos
            """, new { hoy, hastaMantenimientos }, tx, cancellationToken: ct));
        await tx.CommitAsync(ct);
        return citas + mantenimientos;
    }
}
