namespace FollowMotors.Application.Commands.Autenticacion;

using MediatR;
using FollowMotors.Application.Models.Autenticacion;

/// <summary>Solicita autenticación con un identificador interno de FollowMotors.</summary>
public sealed record IniciarSesionCommand(string Identificador, string Contrasena) : IRequest<SesionCliente>;

/// <summary>Revoca exclusivamente la sesión de la solicitud autenticada.</summary>
public sealed record CerrarSesionCommand : IRequest;
