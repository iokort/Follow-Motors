namespace FollowMotors.Infrastructure.Repositories.Clientes;

using Dapper;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Domain.Entities.Clientes;
using FollowMotors.Infrastructure.Database;

/// <summary>Gestiona las cuentas almacenadas en la tabla usuarios.</summary>
public sealed class RepositorioCuentas(ConexionMySql baseDatos) : IRepositorioCuentas
{
    /// <summary>Obtiene credenciales y estado para verificar una sesión.</summary>
    public async Task<CuentaCliente?> BuscarPorIdentificadorAsync(string identificador, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        return await conexion.QuerySingleOrDefaultAsync<CuentaCliente>(new CommandDefinition("""
            SELECT id AS Id, identificador AS Identificador, hash_contrasena AS HashContrasena,
                rol AS Rol, activo AS Activo, creado_en AS CreadoEn
            FROM usuarios WHERE identificador = @identificador LIMIT 1
            """, new { identificador }, cancellationToken: ct));
    }


    /// <summary>Consulta nuevamente el estado de la cuenta para revocar sesiones inactivas.</summary>
    public async Task<CuentaCliente?> ObtenerPorIdAsync(ulong usuarioId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        return await conexion.QuerySingleOrDefaultAsync<CuentaCliente>(new CommandDefinition("""
            SELECT id AS Id, identificador AS Identificador, hash_contrasena AS HashContrasena,
                rol AS Rol, activo AS Activo, creado_en AS CreadoEn
            FROM usuarios WHERE id = @usuarioId LIMIT 1
            """, new { usuarioId }, cancellationToken: ct));
    }

    /// <summary>Inicializa la primera cuenta gerencial solo al arrancar por configuración local.</summary>
    public async Task<bool> CrearGerenteInicialAsync(string identificador, string hashContrasena,
        string nombres, string apellidos, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        // El bloqueo nombrado impide dos altas iniciales simultáneas entre instancias.
        int bloqueo = await conexion.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT GET_LOCK(SHA2(CONCAT(DATABASE(), ':gerente'), 256), 5)", cancellationToken: ct));
        if (bloqueo != 1) throw new InvalidOperationException("No se obtuvo el bloqueo de inicialización.");
        try
        {
            await using var transaccion = await conexion.BeginTransactionAsync(ct);
            var existe = await conexion.ExecuteScalarAsync<int>(new CommandDefinition(
                "SELECT EXISTS(SELECT 1 FROM usuarios WHERE rol = 'GERENTE' AND activo = 1)",
                transaction: transaccion, cancellationToken: ct));
            if (existe == 1) return false;
            await conexion.ExecuteAsync(new CommandDefinition("""
                INSERT INTO usuarios (identificador, hash_contrasena, rol, activo)
                VALUES (@identificador, @hashContrasena, 'GERENTE', 1)
                """, new { identificador, hashContrasena }, transaction: transaccion, cancellationToken: ct));
            ulong usuarioId = await conexion.ExecuteScalarAsync<ulong>(new CommandDefinition(
                "SELECT LAST_INSERT_ID()", transaction: transaccion, cancellationToken: ct));
            await conexion.ExecuteAsync(new CommandDefinition("""
                INSERT INTO empleados (usuario_id, nombres, apellidos, es_mecanico, disponible)
                VALUES (@usuarioId, @nombres, @apellidos, 0, 1)
                """, new { usuarioId, nombres, apellidos }, transaction: transaccion, cancellationToken: ct));
            await AuditoriaSql.RegistrarAsync(conexion, transaccion, null, "usuarios", usuarioId,
                "ALTA_GERENTE_INICIAL", null, "{\"rol\":\"GERENTE\"}", ct);
            await transaccion.CommitAsync(ct);
            return true;
        }
        finally
        {
            await conexion.ExecuteAsync(new CommandDefinition(
                "SELECT RELEASE_LOCK(SHA2(CONCAT(DATABASE(), ':gerente'), 256))", cancellationToken: CancellationToken.None));
        }
    }
}
