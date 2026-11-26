namespace FollowMotors.Application.Abstractions.Repositories.Clientes;

using FollowMotors.Domain.Entities.Clientes;

/// <summary>Define las operaciones de almacenamiento de propietarios y cuentas vinculadas.</summary>
public interface IRepositorioClientes
{
    /// <summary>Registra cuenta y propietario en una misma transacción.</summary>
    Task<ulong> CrearAsync(CuentaCliente cuenta, Cliente cliente, ulong usuarioActorId, CancellationToken ct);

    /// <summary>Desactiva una cuenta conservando el expediente histórico y auditando el cambio.</summary>
    Task<bool> DesactivarAsync(ulong clienteId, ulong usuarioActorId, CancellationToken ct);

    /// <summary>Obtiene el propietario por el identificador de su cuenta.</summary>
    Task<Cliente?> ObtenerPorUsuarioAsync(ulong usuarioId, CancellationToken ct);

    /// <summary>Obtiene un propietario por su identificador interno.</summary>
    Task<Cliente?> ObtenerPorIdAsync(ulong id, CancellationToken ct);
}
