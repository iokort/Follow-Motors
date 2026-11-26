namespace FollowMotors.Application.Handlers.Atenciones;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Atenciones;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Abstractions.Repositories.Personal;
using FollowMotors.Application.Commands.Atenciones;
using FollowMotors.Application.Operaciones;
using FollowMotors.Application.Queries.Atenciones;
using FollowMotors.Domain.Entities.Atenciones;
using FollowMotors.Domain.Rules.Atenciones;
using FollowMotors.Domain.Rules.Seguridad;
using MediatR;

/// <summary>Orquesta el ingreso, la revisión y el cobro fijo con control de roles y asignación.</summary>
public sealed class RecepcionTallerHandler(IRepositorioAtenciones atenciones, IRepositorioPersonal personal,
    IRepositorioClientes clientes, IUsuarioActual usuario, IPoliticaPermisos permisos, IPoliticaAtenciones reglas,
    TimeProvider reloj, TimeZoneInfo zona)
    : CasoUsoTaller(usuario), IRequestHandler<RecepcionarMotocicletaCommand, ulong>,
    IRequestHandler<RegistrarDiagnosticoCommand, bool>, IRequestHandler<CobrarRevisionCommand, ulong>,
    IRequestHandler<ConsultarAtencionQuery, AtencionTaller?>, IRequestHandler<CerrarRevisionCommand>,
    IRequestHandler<ConfigurarTarifaRevisionCommand, ulong>
{
    /// <summary>Registra un ingreso y asigna al mecánico de menor carga dentro de la transacción.</summary>
    public Task<ulong> Handle(RecepcionarMotocicletaCommand solicitud, CancellationToken ct)
    {
        var actor = Empleado();
        var atencion = new AtencionTaller(0, solicitud.ClienteId, solicitud.MotocicletaId, solicitud.CitaId,
            0, solicitud.Motivo.Trim(), solicitud.KilometrajeIngreso, null, "RECIBIDA", reloj.GetUtcNow().UtcDateTime, null);
        return atenciones.CrearAsync(atencion, actor.UsuarioId, ct);
    }

    /// <summary>Restringe el diagnóstico al mecánico asignado o al gerente y audita los hallazgos.</summary>
    public async Task<bool> Handle(RegistrarDiagnosticoCommand solicitud, CancellationToken ct)
    {
        var actor = Empleado();
        var atencion = await ObtenerAtencion(solicitud.AtencionId, ct);
        var empleado = await personal.ObtenerPorUsuarioAsync(actor.UsuarioId, ct);
        if (!permisos.PuedeDiagnosticar(actor.Rol, empleado?.Id, atencion.MecanicoId))
            throw new UnauthorizedAccessException("Solo el mecánico asignado o el gerente registra el diagnóstico.");
        reglas.ExigirDiagnosticoPermitido(atencion);
        return await atenciones.RegistrarDiagnosticoAsync(atencion.Id, solicitud.Hallazgos.Trim(),
            actor.UsuarioId, ct);
    }

    /// <summary>Registra el cobro único el día del diagnóstico con la tarifa vigente en el taller.</summary>
    public async Task<ulong> Handle(CobrarRevisionCommand solicitud, CancellationToken ct)
    {
        var actor = Empleado();
        var atencion = await ObtenerAtencion(solicitud.AtencionId, ct);
        reglas.ExigirCobroPermitido(atencion);
        var ahora = TimeZoneInfo.ConvertTime(reloj.GetUtcNow(), zona).Date;
        if (atencion.DiagnosticadaEn is not DateTime diagnostico ||
            TimeZoneInfo.ConvertTimeFromUtc(diagnostico, zona).Date != ahora)
            throw new InvalidOperationException("La revisión se cobra el día en que fue diagnosticada.");
        return await atenciones.CobrarRevisionAsync(atencion.Id, actor.UsuarioId,
            solicitud.ReferenciaComprobante?.Trim(), ct);
    }

    /// <summary>Devuelve una atención al propietario o al personal, ocultando los registros ajenos.</summary>
    public async Task<AtencionTaller?> Handle(ConsultarAtencionQuery consulta, CancellationToken ct)
    {
        var actor = Actor();
        var atencion = await atenciones.ObtenerAsync(consulta.AtencionId, ct);
        if (atencion is null) return null;
        var cliente = actor.Rol == "CLIENTE" ? await clientes.ObtenerPorUsuarioAsync(actor.UsuarioId, ct) : null;
        return permisos.PuedeConsultarPropiedad(actor.Rol, cliente?.Id, atencion.ClienteId) ? atencion : null;
    }

    /// <summary>Cierra la revisión pagada sin crear un contrato de reparación.</summary>
    public Task Handle(CerrarRevisionCommand solicitud, CancellationToken ct) =>
        atenciones.CerrarSinReparacionAsync(solicitud.AtencionId, Empleado().UsuarioId, ct);

    /// <summary>Actualiza la tarifa fija con vigencia auditable para gerencia.</summary>
    public Task<ulong> Handle(ConfigurarTarifaRevisionCommand solicitud, CancellationToken ct) =>
        atenciones.ConfigurarTarifaAsync(solicitud.Importe, Gerente().UsuarioId, ct);

    /// <summary>Obtiene la atención que va a modificar el personal.</summary>
    private async Task<AtencionTaller> ObtenerAtencion(ulong id, CancellationToken ct) =>
        await atenciones.ObtenerAsync(id, ct) ?? throw new KeyNotFoundException("No existe la atención.");
}
