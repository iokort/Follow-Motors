namespace FollowMotors.Application.Handlers.Atenciones;

using MediatR;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Atenciones;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Queries.Atenciones;
using FollowMotors.Domain.Entities.Atenciones;
using FollowMotors.Domain.Rules.Seguridad;

/// <summary>Consulta atenciones respetando el rol y la propiedad del expediente.</summary>
public sealed class ConsultarAtencionHandler(IRepositorioAtenciones atenciones,
    IRepositorioClientes clientes, IUsuarioActual usuario, IPoliticaPermisos permisos)
    : IRequestHandler<ConsultarAtencionQuery, AtencionTaller?>
{
    /// <summary>Evita que un cliente pueda consultar una atención ajena cambiando el ID.</summary>
    public async Task<AtencionTaller?> Handle(ConsultarAtencionQuery request, CancellationToken cancellationToken)
    {
        if (usuario.UsuarioId is null)
            throw new UnauthorizedAccessException("Es necesaria una sesión válida.");
        var atencion = await atenciones.ObtenerAsync(request.AtencionId, cancellationToken);
        if (atencion is null) return null;
        ulong? clienteId = usuario.Rol == "CLIENTE"
            ? (await clientes.ObtenerPorUsuarioAsync(usuario.UsuarioId.Value, cancellationToken))?.Id : null;
        if (!permisos.PuedeConsultarPropiedad(usuario.Rol, clienteId, atencion.ClienteId))
            throw new UnauthorizedAccessException("No tienes acceso a esta atención.");
        return atencion;
    }
}
