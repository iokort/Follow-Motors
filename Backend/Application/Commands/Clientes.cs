namespace FollowMotors.Application.Commands.Clientes;

using MediatR;

/// <summary>Solicita el alta presencial de una cuenta de cliente sin exigir moto.</summary>
public sealed record CrearCuentaClienteCommand(string Identificador, string Contrasena,
    string Nombres, string Apellidos, string? Telefono) : IRequest<ulong>;

/// <summary>Solicita la desactivación de una cuenta sin borrar su expediente.</summary>
public sealed record DesactivarCuentaClienteCommand(ulong ClienteId) : IRequest<bool>;

/// <summary>Cambia datos de contacto de un expediente sin editar la identidad de su cuenta.</summary>
public sealed record DatosCliente(string Nombres, string Apellidos, string? Telefono);

/// <summary>Actualiza datos verificados presencialmente por el personal.</summary>
public sealed record ActualizarClienteCommand(ulong Id, DatosCliente Datos) : IRequest;
