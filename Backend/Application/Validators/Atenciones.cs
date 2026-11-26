namespace FollowMotors.Application.Validators.Atenciones;

using FluentValidation;
using FollowMotors.Application.Commands.Atenciones;
using FollowMotors.Application.Queries.Atenciones;

/// <summary>Verifica los datos de entrada de CobrarRevisionCommand antes de ejecutar el Handler.</summary>
public sealed class CobrarRevisionValidator : AbstractValidator<CobrarRevisionCommand>
{
    /// <summary>Declara las restricciones que deberá cumplir la solicitud.</summary>
    public CobrarRevisionValidator()
    {
        RuleFor(x => x.AtencionId).GreaterThan(0UL);
        RuleFor(x => x.ReferenciaComprobante).MaximumLength(120);
    }
}

/// <summary>Verifica que el identificador de atención consultado sea válido.</summary>
public sealed class ConsultarAtencionValidator : AbstractValidator<ConsultarAtencionQuery>
{
    /// <summary>Registra la regla mínima de consulta.</summary>
    public ConsultarAtencionValidator() => RuleFor(x => x.AtencionId).GreaterThan(0UL);
}

/// <summary>Verifica los datos de entrada de RecepcionarMotocicletaCommand antes de ejecutar el Handler.</summary>
public sealed class RecepcionarMotocicletaValidator : AbstractValidator<RecepcionarMotocicletaCommand>
{
    /// <summary>Declara las restricciones que deberá cumplir la solicitud.</summary>
    public RecepcionarMotocicletaValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0UL);
        RuleFor(x => x.MotocicletaId).GreaterThan(0UL);
        RuleFor(x => x.MecanicoId).GreaterThan(0UL);
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.CitaId).Must(x => !x.HasValue || x.Value > 0);
    }
}

/// <summary>Verifica los datos de entrada de RegistrarDiagnosticoCommand antes de ejecutar el Handler.</summary>
public sealed class RegistrarDiagnosticoValidator : AbstractValidator<RegistrarDiagnosticoCommand>
{
    /// <summary>Declara las restricciones que deberá cumplir la solicitud.</summary>
    public RegistrarDiagnosticoValidator()
    {
        RuleFor(x => x.AtencionId).GreaterThan(0UL);
        RuleFor(x => x.Hallazgos).NotEmpty().MaximumLength(10000);
    }
}
