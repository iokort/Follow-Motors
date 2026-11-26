namespace FollowMotors.Application.Handlers.Clientes;

using MediatR;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Commands.Clientes;

/// <summary>Tramita altas presenciales sin persistencia directa.</summary>
public sealed class CrearCuentaClienteHandler(IRegistroCliente servicio)
    : IRequestHandler<CrearCuentaClienteCommand, ulong>
{
    /// <summary>Delega el registro a la interfaz del caso de uso.</summary>
    public Task<ulong> Handle(CrearCuentaClienteCommand request, CancellationToken cancellationToken) =>
        servicio.CrearAsync(request, cancellationToken);
}
