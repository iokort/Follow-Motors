namespace FollowMotors.Application.Handlers.Clientes;

using MediatR;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Commands.Clientes;

/// <summary>Coordina la desactivación gerencial sin eliminar datos.</summary>
public sealed class DesactivarCuentaClienteHandler(IRegistroCliente servicio)
    : IRequestHandler<DesactivarCuentaClienteCommand, bool>
{
    /// <summary>Solicita la baja lógica a la interfaz de registro de clientes.</summary>
    public Task<bool> Handle(DesactivarCuentaClienteCommand request, CancellationToken cancellationToken) =>
        servicio.DesactivarAsync(request.ClienteId, cancellationToken);
}
