namespace FollowMotors.Application.Handlers.Personal;

using MediatR;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Personal;
using FollowMotors.Application.Commands.Personal;
using FollowMotors.Domain.Rules.Seguridad;

/// <summary>Permite que un gerente autorizado registre personal operativo.</summary>
public sealed class RegistrarEmpleadoHandler(IRepositorioPersonal personal,
    IProtectorContrasenas protector, IUsuarioActual usuario, IPoliticaPermisos permisos)
    : IRequestHandler<RegistrarEmpleadoCommand, ulong>
{
    /// <summary>Crea la cuenta y el perfil de empleado bajo la política del gerente.</summary>
    public Task<ulong> Handle(RegistrarEmpleadoCommand request, CancellationToken cancellationToken)
    {
        if (!permisos.PuedeAdministrarPersonal(usuario.Rol) || usuario.UsuarioId is null)
            throw new UnauthorizedAccessException("Solo el gerente administra al personal.");
        return personal.RegistrarAsync(request.Identificador.Trim().ToLowerInvariant(),
            protector.GenerarHash(request.Contrasena), request.Nombres.Trim(),
            request.Apellidos.Trim(), request.EsMecanico, usuario.UsuarioId.Value, cancellationToken);
    }
}
