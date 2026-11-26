namespace FollowMotors.Application.Validators.Autenticacion;

using FluentValidation;
using FollowMotors.Application.Commands.Autenticacion;

/// <summary>Verifica los datos de entrada de IniciarSesionCommand antes de ejecutar el Handler.</summary>
public sealed class IniciarSesionValidator : AbstractValidator<IniciarSesionCommand>
{
    /// <summary>Declara las restricciones que deberá cumplir la solicitud.</summary>
    public IniciarSesionValidator()
    {
        RuleFor(x => x.Identificador).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Contrasena).NotEmpty();
    }
}
