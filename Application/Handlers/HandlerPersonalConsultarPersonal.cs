namespace FollowMotors.Application.Handlers.Personal;

using MediatR;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Personal;
using FollowMotors.Application.Queries.Personal;
using FollowMotors.Domain.Entities.Personal;
using FollowMotors.Domain.Rules.Seguridad;

/// <summary>Consulta el personal sin enviar información de credenciales.</summary>
public sealed class ConsultarPersonalHandler(IRepositorioPersonal personal,
    IUsuarioActual usuario, IPoliticaPermisos permisos)
    : IRequestHandler<ConsultarPersonalQuery, IReadOnlyList<Empleado>>
{
    /// <summary>Exige rol de gerente para consultar la plantilla completa.</summary>
    public Task<IReadOnlyList<Empleado>> Handle(ConsultarPersonalQuery request, CancellationToken cancellationToken)
    {
        if (!permisos.PuedeAdministrarPersonal(usuario.Rol))
            throw new UnauthorizedAccessException("Solo el gerente consulta la plantilla.");
        return personal.ListarAsync(cancellationToken);
    }
}
