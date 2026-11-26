namespace FollowMotors.Application.Commands.Clientes;

using MediatR;

/// <summary>Solicita el alta presencial de una cuenta de cliente sin exigir moto.</summary>
public sealed record CrearCuentaClienteCommand(string Identificador, string Contrasena,
    string Nombres, string Apellidos, string? Telefono) : IRequest<ulong>;
