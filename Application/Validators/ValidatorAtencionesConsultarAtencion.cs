namespace FollowMotors.Application.Validators.Atenciones;

using FluentValidation;
using FollowMotors.Application.Queries.Atenciones;

/// <summary>Verifica que el identificador de atención consultado sea válido.</summary>
public sealed class ConsultarAtencionValidator : AbstractValidator<ConsultarAtencionQuery>
{
    /// <summary>Registra la regla mínima de consulta.</summary>
    public ConsultarAtencionValidator() => RuleFor(x => x.AtencionId).GreaterThan(0UL);
}
