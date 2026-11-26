namespace FollowMotors.Application.Agenda;

using FluentValidation;
using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Operaciones;
using MediatR;

/// <summary>Coordina citas y franjas con los permisos y el reloj de la aplicación.</summary>
public sealed class AgendaHandler(IRepositorioAgenda agenda, IUsuarioActual usuario, TimeProvider reloj)
    : CasoUsoTaller(usuario), IRequestHandler<GuardarFranjaCommand, ulong>,
    IRequestHandler<FranjasQuery, IReadOnlyList<FranjaAgenda>>, IRequestHandler<RegistrarCitaCommand, ulong>,
    IRequestHandler<ReprogramarCitaCommand>, IRequestHandler<CerrarCitaCommand>,
    IRequestHandler<CitasQuery, IReadOnlyList<CitaAgenda>>, IRequestHandler<CitaQuery, CitaAgenda>
{
    /// <summary>Configura la capacidad de recepción para personal autorizado.</summary>
    public Task<ulong> Handle(GuardarFranjaCommand solicitud, CancellationToken ct) =>
        agenda.GuardarFranjaAsync(solicitud, Empleado().UsuarioId, ct);

    /// <summary>Consulta cupos del intervalo de forma independiente de la carga de mecánicos.</summary>
    public Task<IReadOnlyList<FranjaAgenda>> Handle(FranjasQuery consulta, CancellationToken ct)
    {
        Empleado();
        if (consulta.Desde >= consulta.Hasta || consulta.Hasta - consulta.Desde > TimeSpan.FromDays(93))
            throw new ArgumentException("El rango de agenda debe abarcar entre un instante y 93 días.");
        return agenda.FranjasAsync(consulta, reloj.GetUtcNow().UtcDateTime, ct);
    }

    /// <summary>Registra una cita interna sin abrir un servicio ni generar un pago.</summary>
    public Task<ulong> Handle(RegistrarCitaCommand solicitud, CancellationToken ct) =>
        agenda.RegistrarAsync(solicitud, Empleado().UsuarioId, reloj.GetUtcNow().UtcDateTime, ct);

    /// <summary>Aplica un cambio al horario de una cita todavía confirmada.</summary>
    public Task Handle(ReprogramarCitaCommand solicitud, CancellationToken ct) =>
        agenda.ReprogramarAsync(solicitud, Empleado().UsuarioId, reloj.GetUtcNow().UtcDateTime, ct);

    /// <summary>Registra el estado de cierre sin cambiar el historial de otras citas.</summary>
    public Task Handle(CerrarCitaCommand solicitud, CancellationToken ct) =>
        agenda.CerrarAsync(solicitud, Empleado().UsuarioId, reloj.GetUtcNow().UtcDateTime, ct);

    /// <summary>Devuelve la agenda accesible para los filtros indicados.</summary>
    public Task<IReadOnlyList<CitaAgenda>> Handle(CitasQuery consulta, CancellationToken ct)
    {
        if (consulta.Pagina is < 1 or > 100_000 || consulta.Desde >= consulta.Hasta)
            throw new ArgumentException("La página o el intervalo no es válido.");
        return agenda.CitasAsync(consulta, Actor(), ct);
    }

    /// <summary>Resuelve la cita sin distinguir entre inexistencia y falta de propiedad.</summary>
    public async Task<CitaAgenda> Handle(CitaQuery consulta, CancellationToken ct) =>
        await agenda.CitaAsync(consulta.Id, Actor(), ct)
        ?? throw new KeyNotFoundException("No existe la cita.");
}

/// <summary>Verifica el formato de los comandos de agenda antes de abrir transacciones.</summary>
public sealed class FranjaAgendaValidator : AbstractValidator<GuardarFranjaCommand>
{
    /// <summary>Exige un intervalo representable y una capacidad positiva.</summary>
    public FranjaAgendaValidator()
    {
        RuleFor(x => x.Id).Must(id => id is null or > 0);
        RuleFor(x => x.Datos.Capacidad).GreaterThan((ushort)0);
        RuleFor(x => x.Datos.Inicio).Must(x => x.UtcDateTime.Year >= 1000);
        RuleFor(x => x.Datos.Fin).GreaterThan(x => x.Datos.Inicio);
    }
}

/// <summary>Comprueba datos obligatorios y límites de la cita registrada.</summary>
public sealed class RegistroCitaValidator : AbstractValidator<RegistrarCitaCommand>
{
    /// <summary>Exige una franja, un expediente y una unidad válidos en formato.</summary>
    public RegistroCitaValidator()
    {
        RuleFor(x => x.FranjaId).GreaterThan(0UL);
        RuleFor(x => x.ClienteId).GreaterThan(0UL);
        RuleFor(x => x.MotocicletaId).GreaterThan(0UL);
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(400);
    }
}
