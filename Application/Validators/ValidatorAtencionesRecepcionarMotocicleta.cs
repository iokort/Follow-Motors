namespace FollowMotors.Application.Validators.Atenciones;

using FluentValidation;
using FollowMotors.Application.Commands.Atenciones;

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
