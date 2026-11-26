namespace FollowMotors.Application.Commands.Clientes;

using MediatR;

/// <summary>Solicita el alta presencial de una cuenta de cliente sin exigir moto.</summary>
public sealed record CrearCuentaClienteCommand(string Identificador, string Contrasena,
    string Nombres, string Apellidos, string? Telefono) : IRequest<ulong>;

/// <summary>Solicita la desactivación de una cuenta sin borrar su expediente.</summary>
public sealed record DesactivarCuentaClienteCommand(ulong ClienteId) : IRequest<bool>;
