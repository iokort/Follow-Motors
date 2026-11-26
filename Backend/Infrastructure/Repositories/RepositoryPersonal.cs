namespace FollowMotors.Infrastructure.Repositories.Personal;

using System.Text.Json;
using Dapper;
using FollowMotors.Application.Abstractions.Repositories.Personal;
using FollowMotors.Domain.Entities.Personal;
using FollowMotors.Infrastructure.Database;

/// <summary>Guarda perfiles de empleado y consulta el personal del taller.</summary>
public sealed class RepositorioPersonal(ConexionMySql baseDatos) : IRepositorioPersonal
{
    internal const string Seleccion = """
        SELECT e.id AS Id, e.usuario_id AS UsuarioId, e.nombres AS Nombres,
            e.apellidos AS Apellidos, e.es_mecanico AS EsMecanico,
            e.disponible AS Disponible, u.rol AS Rol
        FROM empleados e JOIN usuarios u ON u.id = e.usuario_id
        """;

    /// <summary>Inserta cuenta EMPLEADO y perfil en una sola transacción.</summary>
    public async Task<ulong> RegistrarAsync(string identificador, string hashContrasena,
        string nombres, string apellidos, bool esMecanico, ulong usuarioActorId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        await using var transaccion = await conexion.BeginTransactionAsync(ct);
        await conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO usuarios (identificador, hash_contrasena, rol, activo)
            VALUES (@identificador, @hashContrasena, 'EMPLEADO', 1)
            """, new { identificador, hashContrasena }, transaction: transaccion, cancellationToken: ct));
        ulong usuarioId = await conexion.ExecuteScalarAsync<ulong>(new CommandDefinition(
            "SELECT LAST_INSERT_ID()", transaction: transaccion, cancellationToken: ct));
        await conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO empleados (usuario_id, nombres, apellidos, es_mecanico, disponible)
            VALUES (@usuarioId, @nombres, @apellidos, @esMecanico, 1)
            """, new { usuarioId, nombres, apellidos, esMecanico }, transaction: transaccion, cancellationToken: ct));
        ulong empleadoId = await conexion.ExecuteScalarAsync<ulong>(new CommandDefinition(
            "SELECT LAST_INSERT_ID()", transaction: transaccion, cancellationToken: ct));
        await AuditoriaSql.RegistrarAsync(conexion, transaccion, usuarioActorId, "empleados", empleadoId,
            "ALTA_PERSONAL", null, JsonSerializer.Serialize(new { empleadoId, esMecanico }), ct);
        await transaccion.CommitAsync(ct);
        return empleadoId;
    }

    /// <summary>Recupera gerentes, empleados y mecánicos sin incluir contraseñas.</summary>
    public async Task<IReadOnlyList<Empleado>> ListarAsync(CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        var filas = await conexion.QueryAsync<Empleado>(new CommandDefinition(
            Seleccion + " WHERE u.activo ORDER BY e.apellidos, e.nombres",
            cancellationToken: ct));
        return filas.AsList();
    }

    /// <summary>Obtiene el perfil del usuario para comprobar su asignación de atención.</summary>
    public async Task<Empleado?> ObtenerPorUsuarioAsync(ulong usuarioId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        return await conexion.QuerySingleOrDefaultAsync<Empleado>(new CommandDefinition(
            Seleccion + " WHERE u.activo AND e.usuario_id = @usuarioId",
            new { usuarioId }, cancellationToken: ct));
    }
}
