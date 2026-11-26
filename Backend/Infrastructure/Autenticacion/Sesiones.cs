namespace FollowMotors.Infrastructure.Autenticacion;

using Dapper;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Infrastructure.Database;

/// <summary>Guarda identificadores de sesión en MySQL para que el cierre sobreviva a reinicios.</summary>
public sealed class RepositorioSesiones(ConexionMySql baseDatos) : IRepositorioSesiones
{
    /// <inheritdoc />
    public async Task<bool> CrearAsync(ulong usuarioId, string rol, string sesionId, DateTime expiraEn, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        return await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO sesiones_usuarios (id, usuario_id, expira_en)
            SELECT UNHEX(@sesionId), id, @expiraEn FROM usuarios
            WHERE id = @usuarioId AND rol = @rol AND activo
            """, new { usuarioId, rol, sesionId, expiraEn }, cancellationToken: ct)) == 1;
    }

    /// <inheritdoc />
    public async Task<bool> EsValidaAsync(ulong usuarioId, string rol, string sesionId, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        return await db.ExecuteScalarAsync<bool>(new CommandDefinition("""
            SELECT EXISTS(SELECT 1 FROM sesiones_usuarios s JOIN usuarios u ON u.id = s.usuario_id
                WHERE s.id = UNHEX(@sesionId) AND u.id = @usuarioId AND u.rol = @rol
                AND u.activo AND s.expira_en > UTC_TIMESTAMP(3))
            """, new { usuarioId, rol, sesionId }, cancellationToken: ct));
    }

    /// <inheritdoc />
    public async Task RevocarAsync(ulong usuarioId, string sesionId, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await db.ExecuteAsync(new CommandDefinition("""
            DELETE FROM sesiones_usuarios WHERE id = UNHEX(@sesionId) AND usuario_id = @usuarioId
            """, new { usuarioId, sesionId }, cancellationToken: ct));
    }
}
