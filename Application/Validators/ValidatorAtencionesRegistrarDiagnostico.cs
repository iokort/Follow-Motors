namespace FollowMotors.Application.Validators.Atenciones;

using FluentValidation;
using FollowMotors.Application.Commands.Atenciones;

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
