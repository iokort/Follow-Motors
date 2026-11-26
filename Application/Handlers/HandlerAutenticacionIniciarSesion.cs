namespace FollowMotors.Application.Handlers.Autenticacion;

using MediatR;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Commands.Autenticacion;
using FollowMotors.Application.Models.Autenticacion;

/// <summary>Procesa el inicio de sesión mediante un servicio abstracto.</summary>
public sealed class IniciarSesionHandler(IServicioAutenticacion servicio)
    : IRequestHandler<IniciarSesionCommand, SesionCliente>
{
    /// <summary>Autentica la solicitud sin conocer contraseñas almacenadas ni SQL.</summary>
    public Task<SesionCliente> Handle(IniciarSesionCommand request, CancellationToken cancellationToken) =>
        servicio.IniciarSesionAsync(request.Identificador, request.Contrasena, cancellationToken);
}
