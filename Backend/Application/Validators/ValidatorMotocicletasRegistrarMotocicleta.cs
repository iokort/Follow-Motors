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

/// <summary>Reutiliza el formato de ficha que aplica el registro de una motocicleta.</summary>
public sealed class ActualizarMotocicletaValidator : AbstractValidator<ActualizarMotocicletaCommand>
{
    /// <summary>Comprueba el ID de ruta y valida la ficha con las reglas compartidas.</summary>
    public ActualizarMotocicletaValidator(IValidator<RegistrarMotocicletaCommand> ficha)
    {
        RuleFor(x => x.Id).GreaterThan(0UL);
        RuleFor(x => x.Datos).NotNull().SetValidator(ficha);
    }
}
