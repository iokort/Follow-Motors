namespace FollowMotors.Application.Handlers.Gerencia;

using MediatR;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Personal;
using FollowMotors.Application.Models.Gerencia;
using FollowMotors.Application.Queries.Gerencia;
using FollowMotors.Domain.Entities.Personal;
using FollowMotors.Domain.Rules.Seguridad;

/// <summary>Resuelve la consulta del gerente por interfaces, sin depender de MySQL ni de HTTP.</summary>
public sealed class ConsultarPerfilGerenteHandler(
    IUsuarioActual usuario,
    IRepositorioPersonal personal,
    IPoliticaPermisos permisos)
    : IRequestHandler<ConsultarPerfilGerenteQuery, PerfilGerente>
{
    /// <summary>Comprueba el rol y recupera exclusivamente el perfil asociado a la sesión.</summary>
    public async Task<PerfilGerente> Handle(ConsultarPerfilGerenteQuery request,
        CancellationToken cancellationToken)
    {
        if (!permisos.PuedeAdministrarPersonal(usuario.Rol) || usuario.UsuarioId is null)
            throw new UnauthorizedAccessException("Solo el gerente puede consultar este perfil.");

        var empleado = await personal.ObtenerPorUsuarioAsync(usuario.UsuarioId.Value, cancellationToken);
        if (empleado is not Gerente gerente)
            throw new KeyNotFoundException("No existe un perfil gerencial asociado a esta cuenta.");

        return new PerfilGerente(gerente.Id, gerente.Nombres,
            gerente.Apellidos, gerente.Disponible);
    }
}
