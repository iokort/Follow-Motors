namespace FollowMotors.Application.Operaciones;

using FluentValidation;
using FollowMotors.Application.Abstractions.Autenticacion;
using MediatR;

/// <summary>Solicita las revisiones y servicios asociados a una unidad con historial propio.</summary>
public sealed record HistorialMotocicletaQuery(ulong MotocicletaId, int Pagina = 1)
    : IRequest<IReadOnlyList<EntradaHistorial>>;

/// <summary>Expone una atención histórica, incluidos diagnóstico, pago y estado real de reparación.</summary>
public sealed record EntradaHistorial(ulong AtencionId, string EstadoAtencion, uint KilometrajeIngreso,
    ulong MecanicoId, string Motivo, string? Diagnostico, DateTime Ingreso, DateTime? DiagnosticadaEn,
    decimal? TarifaCobrada, DateTime? CobradoEn, ulong? OrdenId, string? EstadoOrden,
    decimal? Anticipo, DateTime? TerminadaEn, DateTime? EntregadaEn);

/// <summary>Resuelve un mantenimiento programado a partir de una entrega comprobada o una cancelación.</summary>
public sealed record ResolverMantenimientoCommand(ulong Id, bool Realizado, ulong? OrdenId) : IRequest;

/// <summary>Solicita los indicadores gerenciales del intervalo de fechas indicado.</summary>
public sealed record ReporteOperativoQuery(DateTimeOffset Desde, DateTimeOffset Hasta)
    : IRequest<IReadOnlyList<IndicadorOperativo>>;

/// <summary>Total por módulo y estado a partir de los registros persistidos.</summary>
public sealed record IndicadorOperativo(string Modulo, string Estado, long Total);

/// <summary>Lecturas de seguimiento y validación de los mantenimientos programados.</summary>
public interface IRepositorioSeguimiento
{
    /// <summary>Consulta las revisiones y reparaciones de la unidad accesible para la sesión.</summary>
    Task<IReadOnlyList<EntradaHistorial>> HistorialAsync(ulong id, int pagina, ActorTaller actor, CancellationToken ct);

    /// <summary>Consolida el estado actual de la agenda, órdenes, cancelaciones y mantenimientos.</summary>
    Task<IReadOnlyList<IndicadorOperativo>> ReporteAsync(DateTime desde, DateTime hasta, CancellationToken ct);

    /// <summary>Persiste el estado de un mantenimiento y la evidencia de la orden entregada.</summary>
    Task ResolverMantenimientoAsync(ResolverMantenimientoCommand solicitud, ulong usuarioId, CancellationToken ct);
}

/// <summary>Coordina la consulta por propietario, la validación y el reporte de gerencia.</summary>
public sealed class SeguimientoHandler(IRepositorioSeguimiento seguimiento, IUsuarioActual usuario)
    : CasoUsoTaller(usuario), IRequestHandler<HistorialMotocicletaQuery, IReadOnlyList<EntradaHistorial>>,
      IRequestHandler<ReporteOperativoQuery, IReadOnlyList<IndicadorOperativo>>,
      IRequestHandler<ResolverMantenimientoCommand>
{
    /// <summary>Pagina el historial sin perder las revisiones que no originaron reparaciones.</summary>
    public Task<IReadOnlyList<EntradaHistorial>> Handle(HistorialMotocicletaQuery consulta, CancellationToken ct)
    {
        if (consulta.Pagina is < 1 or > 100_000) throw new ArgumentException("La página no es válida.");
        return seguimiento.HistorialAsync(consulta.MotocicletaId, consulta.Pagina, Actor(), ct);
    }

    /// <summary>Exige gerencia y un rango válido antes de consultar indicadores.</summary>
    public Task<IReadOnlyList<IndicadorOperativo>> Handle(ReporteOperativoQuery consulta, CancellationToken ct)
    {
        Gerente();
        if (consulta.Desde >= consulta.Hasta || consulta.Desde.UtcDateTime.Year < 1000)
            throw new ArgumentException("El periodo del reporte no es válido.");
        return seguimiento.ReporteAsync(consulta.Desde.UtcDateTime, consulta.Hasta.UtcDateTime, ct);
    }

    /// <summary>Registra la decisión de personal responsable sobre un mantenimiento.</summary>
    public Task Handle(ResolverMantenimientoCommand solicitud, CancellationToken ct) =>
        seguimiento.ResolverMantenimientoAsync(solicitud, Empleado().UsuarioId, ct);
}

/// <summary>Impide registrar mantenimientos sin referencia temporal o de kilometraje.</summary>
public sealed class MantenimientoValidator : AbstractValidator<ProgramarMantenimientoCommand>
{
    /// <summary>Valida la motocicleta, la descripción y al menos un criterio de vencimiento.</summary>
    public MantenimientoValidator()
    {
        RuleFor(x => x.MotocicletaId).GreaterThan(0UL);
        RuleFor(x => x.OrdenOrigenId).Must(id => id is null or > 0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(300);
        RuleFor(x => x.FechaEstimada).Must(f => f is null || f.Value.Year >= 1000);
        RuleFor(x => x).Must(x => x.FechaEstimada.HasValue || x.KilometrajeObjetivo.HasValue)
            .WithMessage("Indica una fecha o un kilometraje objetivo.");
    }
}

/// <summary>Comprueba la evidencia requerida para validar la realización de un mantenimiento.</summary>
public sealed class ResolverMantenimientoValidator : AbstractValidator<ResolverMantenimientoCommand>
{
    /// <summary>Exige una orden de trabajo entregada al marcar como realizado.</summary>
    public ResolverMantenimientoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
        RuleFor(x => x.OrdenId).Must(id => id is > 0).When(x => x.Realizado);
    }
}
