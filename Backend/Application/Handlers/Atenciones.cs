namespace FollowMotors.Application.Handlers.Atenciones;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Abstractions.Repositories.Atenciones;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Commands.Atenciones;
using FollowMotors.Application.Queries.Atenciones;
using FollowMotors.Domain.Entities.Atenciones;
using FollowMotors.Domain.Rules.Seguridad;
using MediatR;

/// <summary>Coordina el caso de uso CobrarRevision por medio de una interfaz.</summary>
public sealed class CobrarRevisionHandler(IRecepcionTaller recepcion)
    : IRequestHandler<CobrarRevisionCommand, ulong>
{
    /// <summary>Ejecuta la operación sin acceder directamente a MySQL ni HTTP.</summary>
    public Task<ulong> Handle(CobrarRevisionCommand request, CancellationToken cancellationToken) =>
        recepcion.CobrarAsync(request, cancellationToken);
}

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

/// <summary>Coordina el caso de uso RecepcionarMotocicleta por medio de una interfaz.</summary>
public sealed class RecepcionarMotocicletaHandler(IRecepcionTaller recepcion)
    : IRequestHandler<RecepcionarMotocicletaCommand, ulong>
{
    /// <summary>Ejecuta la operación sin acceder directamente a MySQL ni HTTP.</summary>
    public Task<ulong> Handle(RecepcionarMotocicletaCommand request, CancellationToken cancellationToken) =>
        recepcion.RecibirAsync(request, cancellationToken);
}

/// <summary>Coordina el caso de uso RegistrarDiagnostico por medio de una interfaz.</summary>
public sealed class RegistrarDiagnosticoHandler(IRecepcionTaller recepcion)
    : IRequestHandler<RegistrarDiagnosticoCommand, bool>
{
    /// <summary>Ejecuta la operación sin acceder directamente a MySQL ni HTTP.</summary>
    public Task<bool> Handle(RegistrarDiagnosticoCommand request, CancellationToken cancellationToken) =>
        recepcion.DiagnosticarAsync(request, cancellationToken);
}
