namespace FollowMotors.Application.Services.Atenciones;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Atenciones;
using FollowMotors.Application.Abstractions.Repositories.Personal;
using FollowMotors.Application.Abstractions.Operaciones;
using FollowMotors.Application.Commands.Atenciones;
using FollowMotors.Domain.Entities.Atenciones;
using FollowMotors.Domain.Rules.Atenciones;
using FollowMotors.Domain.Rules.Seguridad;

/// <summary>Coordina las operaciones de recepción y diagnóstico del taller.</summary>
public sealed class RecepcionTaller : IRecepcionTaller
{
    private readonly IRepositorioAtenciones _atenciones;
    private readonly IRepositorioPersonal _personal;
    private readonly IUsuarioActual _usuario;
    private readonly IPoliticaPermisos _permisos;
    private readonly IPoliticaAtenciones _reglas;

    /// <summary>Obtiene contratos de persistencia, identidad y reglas a través del constructor.</summary>
    public RecepcionTaller(IRepositorioAtenciones atenciones, IRepositorioPersonal personal,
        IUsuarioActual usuario, IPoliticaPermisos permisos, IPoliticaAtenciones reglas)
    {
        _atenciones = atenciones;
        _personal = personal;
        _usuario = usuario;
        _permisos = permisos;
        _reglas = reglas;
    }

    /// <summary>Verifica permiso y asigna un único mecánico a una nueva atención.</summary>
    public Task<ulong> RecibirAsync(RecepcionarMotocicletaCommand solicitud, CancellationToken ct)
    {
        ulong actor = ExigirPersonalAutorizado();
        var atencion = new AtencionTaller(0, solicitud.ClienteId, solicitud.MotocicletaId,
            solicitud.CitaId, solicitud.MecanicoId, solicitud.Motivo.Trim(),
            solicitud.KilometrajeIngreso, null, "RECIBIDA", DateTime.UtcNow, null);
        return _atenciones.CrearAsync(atencion, actor, ct);
    }

    /// <summary>Restringe el diagnóstico al mecánico asignado o al gerente.</summary>
    public async Task<bool> DiagnosticarAsync(RegistrarDiagnosticoCommand solicitud, CancellationToken ct)
    {
        ulong actor = ExigirPersonalAutorizado();
        var atencion = await _atenciones.ObtenerAsync(solicitud.AtencionId, ct)
            ?? throw new KeyNotFoundException("No existe la atención.");
        var empleado = await _personal.ObtenerPorUsuarioAsync(actor, ct);
        if (!_permisos.PuedeDiagnosticar(_usuario.Rol, empleado?.Id, atencion.MecanicoId))
            throw new UnauthorizedAccessException("Solo el mecánico asignado registra el diagnóstico.");
        _reglas.ExigirDiagnosticoPermitido(atencion);
        return await _atenciones.RegistrarDiagnosticoAsync(atencion.Id, solicitud.Hallazgos.Trim(), actor, ct);
    }

    /// <summary>Registra un cobro de revisión según la tarifa fija vigente.</summary>
    public async Task<ulong> CobrarAsync(CobrarRevisionCommand solicitud, CancellationToken ct)
    {
        ulong actor = ExigirPersonalAutorizado();
        var atencion = await _atenciones.ObtenerAsync(solicitud.AtencionId, ct)
            ?? throw new KeyNotFoundException("No existe la atención.");
        _reglas.ExigirCobroPermitido(atencion);
        return await _atenciones.CobrarRevisionAsync(solicitud.AtencionId, actor,
            solicitud.ReferenciaComprobante, ct);
    }

    /// <summary>Comprueba identidad y rol en el caso de uso además de la API.</summary>
    private ulong ExigirPersonalAutorizado()
    {
        if (!_permisos.PuedeGestionarAtenciones(_usuario.Rol) || _usuario.UsuarioId is null)
            throw new UnauthorizedAccessException("Operación reservada al personal autorizado.");
        return _usuario.UsuarioId.Value;
    }
}
