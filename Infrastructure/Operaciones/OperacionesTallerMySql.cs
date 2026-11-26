namespace FollowMotors.Infrastructure.Operaciones;

using Dapper;
using FollowMotors.Application.Operaciones;
using FollowMotors.Infrastructure.Database;
using MySqlConnector;

/// <summary>Adaptador MySQL de los casos de uso complementarios del taller.</summary>
/// <remarks>Las reglas están en Domain/Application; aquí se hacen transacciones y cambios atómicos.</remarks>
public sealed partial class OperacionesTallerMySql(ConexionMySql baseDatos) : IOperacionesTaller
{
    /// <summary>Abre una conexión por operación, gestionada por el pool de MySqlConnector.</summary>
    private Task<MySqlConnection> Abrir(CancellationToken ct) => baseDatos.AbrirAsync(ct);

    /// <summary>Recupera el identificador asignado por MySQL en la transacción actual.</summary>
    private static Task<ulong> IdNuevo(MySqlConnection conexion, MySqlTransaction tx, CancellationToken ct) =>
        conexion.ExecuteScalarAsync<ulong>(new CommandDefinition("SELECT LAST_INSERT_ID()",
            transaction: tx, cancellationToken: ct));

    /// <summary>Escribe auditoría vinculada a la misma transacción del cambio.</summary>
    private static Task Auditar(MySqlConnection conexion, MySqlTransaction tx, ActorTaller actor,
        string entidad, ulong registroId, string accion, CancellationToken ct) =>
        conexion.ExecuteAsync(new CommandDefinition(
            "INSERT INTO auditoria(usuario_id,entidad,registro_id,accion) VALUES(@UsuarioId,@entidad,@registroId,@accion)",
            new { actor.UsuarioId, entidad, registroId, accion }, tx, cancellationToken: ct));

    /// <summary>Restringe todas las lecturas privadas a su propietario usando el usuario del token.</summary>
    private const string PropiedadPresupuesto = @"(@Rol <> 'CLIENTE' OR EXISTS
        (SELECT 1 FROM atenciones a JOIN clientes c ON c.id=a.cliente_id
         WHERE a.id=p.atencion_id AND c.usuario_id=@UsuarioId))";
    private const string PropiedadOrden = @"(@Rol <> 'CLIENTE' OR EXISTS
        (SELECT 1 FROM presupuestos p JOIN atenciones a ON a.id=p.atencion_id
         JOIN clientes c ON c.id=a.cliente_id
         WHERE p.id=o.presupuesto_id AND c.usuario_id=@UsuarioId))";
}
