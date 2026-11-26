namespace FollowMotors.Application.Commands.Clientes;

using MediatR;

/// <summary>Solicita la desactivación de una cuenta sin borrar su expediente.</summary>
public sealed record DesactivarCuentaClienteCommand(ulong ClienteId) : IRequest<bool>;
