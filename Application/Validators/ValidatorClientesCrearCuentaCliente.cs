namespace FollowMotors.Application.Validators.Clientes;

using FluentValidation;
using FollowMotors.Application.Commands.Clientes;

/// <summary>Verifica los datos de entrada de CrearCuentaClienteCommand antes de ejecutar el Handler.</summary>
public sealed class CrearCuentaClienteValidator : AbstractValidator<CrearCuentaClienteCommand>
{
    /// <summary>Declara las restricciones que deberá cumplir la solicitud.</summary>
    public CrearCuentaClienteValidator()
    {
        RuleFor(x => x.Identificador).NotEmpty().MaximumLength(120)
            .Must(x => x is not null && x.EndsWith("@follow.motors", StringComparison.OrdinalIgnoreCase))
            .WithMessage("El identificador debe terminar en @follow.motors.");
        RuleFor(x => x.Contrasena).NotEmpty().MinimumLength(12).MaximumLength(256);
        RuleFor(x => x.Nombres).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Telefono).MaximumLength(25);
    }
}
