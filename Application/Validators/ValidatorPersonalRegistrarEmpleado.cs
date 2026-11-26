namespace FollowMotors.Application.Validators.Personal;

using FluentValidation;
using FollowMotors.Application.Commands.Personal;

/// <summary>Verifica los datos de entrada de RegistrarEmpleadoCommand antes de ejecutar el Handler.</summary>
public sealed class RegistrarEmpleadoValidator : AbstractValidator<RegistrarEmpleadoCommand>
{
    /// <summary>Declara las restricciones que deberá cumplir la solicitud.</summary>
    public RegistrarEmpleadoValidator()
    {
        RuleFor(x => x.Identificador).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Contrasena).NotEmpty().MinimumLength(12).MaximumLength(256);
        RuleFor(x => x.Nombres).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(120);
    }
}
