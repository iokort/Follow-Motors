namespace FollowMotors.Application.Abstractions.Operaciones;

using FollowMotors.Application.Commands.Clientes;

/// <summary>Contrato de los casos de uso administrativos de clientes.</summary>
public interface IRegistroCliente
{
    /// <summary>Crea un cliente y su cuenta mediante alta presencial autorizada.</summary>
    Task<ulong> CrearAsync(CrearCuentaClienteCommand solicitud, CancellationToken ct);

    /// <summary>Desactiva la cuenta únicamente cuando el actor es gerente.</summary>
    Task<bool> DesactivarAsync(ulong clienteId, CancellationToken ct);
}
