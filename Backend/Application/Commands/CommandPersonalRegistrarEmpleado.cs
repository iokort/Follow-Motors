namespace FollowMotors.Application.Commands.Personal;

using MediatR;

/// <summary>Solicita al gerente crear un perfil de empleado o mecánico.</summary>
public sealed record RegistrarEmpleadoCommand(string Identificador, string Contrasena,
    string Nombres, string Apellidos, bool EsMecanico) : IRequest<ulong>;
