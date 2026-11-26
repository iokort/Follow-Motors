namespace FollowMotors.Infrastructure.Repositories.Clientes;

using System.Text.Json;
using Dapper;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Domain.Entities.Clientes;
using FollowMotors.Infrastructure.Database;

/// <summary>Implementa la persistencia transaccional del expediente del cliente.</summary>
public sealed class RepositorioClientes(ConexionMySql baseDatos) : IRepositorioClientes
{
    /// <summary>Inserta cuenta, propietario y auditoría de forma atómica.</summary>
    public async Task<ulong> CrearAsync(CuentaCliente cuenta, Cliente cliente, ulong usuarioActorId,
        CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        await using var transaccion = await conexion.BeginTransactionAsync(ct);
        await conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO usuarios (identificador, hash_contrasena, rol, activo)
            VALUES (@Identificador, @HashContrasena, 'CLIENTE', 1)
            """, cuenta, transaction: transaccion, cancellationToken: ct));
        ulong usuarioId = await conexion.ExecuteScalarAsync<ulong>(new CommandDefinition(
            "SELECT LAST_INSERT_ID()", transaction: transaccion, cancellationToken: ct));
        await conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO clientes (usuario_id, nombres, apellidos, telefono, registrado_por_usuario_id)
            VALUES (@usuarioId, @Nombres, @Apellidos, @Telefono, @usuarioActorId)
            """, new { usuarioId, cliente.Nombres, cliente.Apellidos, cliente.Telefono, usuarioActorId },
            transaction: transaccion, cancellationToken: ct));
        ulong clienteId = await conexion.ExecuteScalarAsync<ulong>(new CommandDefinition(
            "SELECT LAST_INSERT_ID()", transaction: transaccion, cancellationToken: ct));
        await AuditoriaSql.RegistrarAsync(conexion, transaccion, usuarioActorId, "clientes", clienteId,
            "ALTA_CLIENTE", null, JsonSerializer.Serialize(new { clienteId, usuarioId }), ct);
        await transaccion.CommitAsync(ct);
        return clienteId;
    }

    /// <summary>Inhabilita la cuenta sin borrar su historial ni el propietario.</summary>
    public async Task<bool> DesactivarAsync(ulong clienteId, ulong usuarioActorId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        await using var transaccion = await conexion.BeginTransactionAsync(ct);
        int actualizadas = await conexion.ExecuteAsync(new CommandDefinition("""
            UPDATE usuarios u INNER JOIN clientes c ON c.usuario_id = u.id
            SET u.activo = 0 WHERE c.id = @clienteId AND u.rol = 'CLIENTE' AND u.activo = 1
            """, new { clienteId }, transaction: transaccion, cancellationToken: ct));
        if (actualizadas == 0) return false;
        await AuditoriaSql.RegistrarAsync(conexion, transaccion, usuarioActorId, "clientes", clienteId,
            "DESACTIVAR_CUENTA", "{\"activo\":true}", "{\"activo\":false}", ct);
        await transaccion.CommitAsync(ct);
        return true;
    }

    /// <summary>Consulta un expediente que todavía está vinculado a una cuenta activa.</summary>
    public async Task<Cliente?> ObtenerPorUsuarioAsync(ulong usuarioId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        return await conexion.QuerySingleOrDefaultAsync<Cliente>(new CommandDefinition("""
            SELECT c.id AS Id, c.usuario_id AS UsuarioId, c.nombres AS Nombres,
                c.apellidos AS Apellidos, c.telefono AS Telefono,
                c.registrado_por_usuario_id AS RegistradoPorUsuarioId, c.creado_en AS CreadoEn
            FROM clientes c JOIN usuarios u ON u.id = c.usuario_id
            WHERE c.usuario_id = @usuarioId AND u.activo = 1
            """, new { usuarioId }, cancellationToken: ct));
    }

    /// <summary>Consulta el expediente por ID, incluso cuando la cuenta está inactiva.</summary>
    public async Task<Cliente?> ObtenerPorIdAsync(ulong id, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        return await conexion.QuerySingleOrDefaultAsync<Cliente>(new CommandDefinition("""
            SELECT id AS Id, usuario_id AS UsuarioId, nombres AS Nombres, apellidos AS Apellidos,
                   telefono AS Telefono, registrado_por_usuario_id AS RegistradoPorUsuarioId,
                   creado_en AS CreadoEn FROM clientes WHERE id = @id
            """, new { id }, cancellationToken: ct));
    }
}
