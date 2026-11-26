namespace FollowMotors.Infrastructure.Database;

using Dapper;
using MySqlConnector;

/// <summary>Guarda la auditoría dentro de la misma transacción del cambio principal.</summary>
internal static class AuditoriaSql
{
    /// <summary>Inserta la traza vinculada al usuario y registro modificados.</summary>
    internal static Task<int> RegistrarAsync(MySqlConnection conexion, MySqlTransaction transaccion,
        ulong? usuarioId, string entidad, ulong registroId, string accion,
        string? antes, string? despues, CancellationToken ct) =>
        conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO auditoria(usuario_id, entidad, registro_id, accion, antes, despues)
            VALUES (@usuarioId, @entidad, @registroId, @accion,
                    @antes, @despues)
            """, new { usuarioId, entidad, registroId, accion, antes, despues },
            transaction: transaccion, cancellationToken: ct));
}
