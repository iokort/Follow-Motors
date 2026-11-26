namespace FollowMotors.Application.Operaciones;

using FluentValidation;
using FollowMotors.Domain.Rules.Operaciones;
using FollowMotors.Domain.Rules.Servicios;

/// <summary>Comprueba que una cotización pueda conservarse con pesos e importes exactos.</summary>
public sealed class PresupuestoValidator : AbstractValidator<CrearPresupuestoCommand>
{
    /// <summary>Valida referencias, importes y el plan de trabajo completo.</summary>
    public PresupuestoValidator()
    {
        RuleFor(x => x.AtencionId).GreaterThan(0UL);
        RuleFor(x => x.TipoServicioId).GreaterThan(0UL);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ManoObra).PrecisionScale(12, 2, true);
        RuleFor(x => x.Refacciones).PrecisionScale(12, 2, true);
        RuleFor(x => x).Must(x => ImportesReparacion.EsTotalValido(x.ManoObra, x.Refacciones))
            .WithMessage("El total de la reparación debe ser positivo y representable.");
        RuleFor(x => x.Tareas).Must(tareas => tareas is not null && tareas.All(t => t is not null)
            && CalculadoraAvance.PlanValido(tareas.Select(t => t.Peso)))
            .WithMessage("Las tareas deben estar definidas y sus pesos sumar exactamente 100 %.");
        RuleForEach(x => x.Tareas).ChildRules(tarea => tarea.RuleFor(t => t.Descripcion).NotEmpty().MaximumLength(300));
        RuleFor(x => x.EntregaEstimada).Must(x => x is null || x.Value.UtcDateTime.Year >= 1000);
    }
}

/// <summary>Valida el pago correspondiente a una nueva contratación.</summary>
public sealed class ContratarServicioValidator : AbstractValidator<ContratarServicioCommand>
{
    /// <summary>Exige un presupuesto, el total por conceptos y un comprobante presencial.</summary>
    public ContratarServicioValidator()
    {
        RuleFor(x => x.PresupuestoId).GreaterThan(0UL);
        RuleFor(x => x.ManoObra).PrecisionScale(12, 2, true);
        RuleFor(x => x.Refacciones).PrecisionScale(12, 2, true);
        RuleFor(x => x).Must(x => ImportesReparacion.EsTotalValido(x.ManoObra, x.Refacciones))
            .WithMessage("El anticipo de reparación debe ser positivo.");
        RuleFor(x => x.Comprobante).NotEmpty().MaximumLength(120);
    }
}

/// <summary>Valida el anticipo al registrar el pago de una orden heredada.</summary>
public sealed class AnticipoValidator : AbstractValidator<RegistrarAnticipoCommand>
{
    /// <summary>Exige un comprobante y las mismas restricciones de importes que la contratación.</summary>
    public AnticipoValidator()
    {
        RuleFor(x => x.OrdenId).GreaterThan(0UL);
        RuleFor(x => x.ManoObra).PrecisionScale(12, 2, true);
        RuleFor(x => x.Refacciones).PrecisionScale(12, 2, true);
        RuleFor(x => x).Must(x => ImportesReparacion.EsTotalValido(x.ManoObra, x.Refacciones))
            .WithMessage("El anticipo de reparación debe ser positivo.");
        RuleFor(x => x.Comprobante).NotEmpty().MaximumLength(120);
    }
}

/// <summary>Exige evidencia para el progreso parcial de una tarea.</summary>
public sealed class AvanceTareaValidator : AbstractValidator<ActualizarTareaCommand>
{
    /// <summary>Verifica el porcentaje y la identificación de los registros que cambiarán.</summary>
    public AvanceTareaValidator()
    {
        RuleFor(x => x.OrdenId).GreaterThan(0UL);
        RuleFor(x => x.TareaId).GreaterThan(0UL);
        RuleFor(x => x.Porcentaje).InclusiveBetween(0m, 100m).PrecisionScale(5, 2, true);
        RuleFor(x => x.Evidencia).MaximumLength(8000);
        RuleFor(x => x.Evidencia).NotEmpty().When(x => x.Porcentaje is > 0 and < 100);
    }
}

/// <summary>Comprueba los datos que necesita una pieza para agregarse al presupuesto.</summary>
public sealed class CotizarRefaccionValidator : AbstractValidator<SolicitarRefaccionCommand>
{
    /// <summary>Valida descripción, cantidad y costo por unidad del proveedor cotizado.</summary>
    public CotizarRefaccionValidator()
    {
        RuleFor(x => x.PresupuestoId).GreaterThan(0UL);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Cantidad).InclusiveBetween(1, ushort.MaxValue);
        RuleFor(x => x.PrecioUnitario).GreaterThanOrEqualTo(0m).PrecisionScale(12, 2, true);
    }
}

/// <summary>Comprueba que una recepción represente un movimiento respaldado por el proveedor.</summary>
public sealed class RecibirRefaccionValidator : AbstractValidator<RecibirRefaccionCommand>
{
    /// <summary>Valida cantidad, costo y referencia de la recepción.</summary>
    public RecibirRefaccionValidator()
    {
        RuleFor(x => x.RefaccionId).GreaterThan(0UL);
        RuleFor(x => x.Cantidad).InclusiveBetween(1, ushort.MaxValue);
        RuleFor(x => x.CostoUnitario).GreaterThanOrEqualTo(0m).PrecisionScale(12, 2, true);
        RuleFor(x => x.Factura).NotEmpty().MaximumLength(150);
    }
}

/// <summary>Rechaza reintegros cuyo importe o comprobante no se puede registrar.</summary>
public sealed class ReembolsoValidator : AbstractValidator<RegistrarReembolsoCommand>
{
    /// <summary>Valida la solicitud, los centavos y la referencia de la devolución manual.</summary>
    public ReembolsoValidator()
    {
        RuleFor(x => x.SolicitudId).GreaterThan(0UL);
        RuleFor(x => x.Importe).GreaterThan(0m).PrecisionScale(12, 2, true);
        RuleFor(x => x.Referencia).NotEmpty().MaximumLength(120);
    }
}

/// <summary>Exige una explicación de cancelación cuando gerencia rechaza la solicitud.</summary>
public sealed class ResolverCancelacionValidator : AbstractValidator<ResolverCancelacionCommand>
{
    /// <summary>Aplica el espacio disponible para el motivo en el esquema vigente.</summary>
    public ResolverCancelacionValidator()
    {
        RuleFor(x => x.SolicitudId).GreaterThan(0UL);
        RuleFor(x => x.MotivoRechazo).NotEmpty().When(x => !x.Aprobar);
        RuleFor(x => x.MotivoRechazo).MaximumLength(300);
    }
}

/// <summary>Exige términos claros cuando el tipo de servicio tiene garantía.</summary>
public sealed class TipoServicioValidator : AbstractValidator<RegistrarTipoServicioCommand>
{
    /// <summary>Valida el catálogo de servicios sin aceptar términos vacíos de cobertura vigente.</summary>
    public TipoServicioValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(130);
        RuleFor(x => x.Descripcion).MaximumLength(4000);
        RuleFor(x => x.Cobertura).MaximumLength(8000);
        RuleFor(x => x.Cobertura).NotEmpty().When(x => x.GarantiaDias > 0);
    }
}
