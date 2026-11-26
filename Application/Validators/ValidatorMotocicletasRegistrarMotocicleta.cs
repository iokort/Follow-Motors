namespace FollowMotors.Application.Validators.Motocicletas;

using FluentValidation;
using FollowMotors.Application.Commands.Motocicletas;

/// <summary>Verifica los datos de entrada de RegistrarMotocicletaCommand antes de ejecutar el Handler.</summary>
public sealed class RegistrarMotocicletaValidator : AbstractValidator<RegistrarMotocicletaCommand>
{
    /// <summary>Declara las restricciones que deberá cumplir la solicitud.</summary>
    public RegistrarMotocicletaValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0UL);
        RuleFor(x => x.NumeroSerie).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Marca).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Modelo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Anio).Must(x => x is null || x is >= 1900 and <= 2150);
        RuleFor(x => x.Placa).MaximumLength(20);
        RuleFor(x => x.Color).MaximumLength(60);
    }
}
