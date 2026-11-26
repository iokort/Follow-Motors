namespace FollowMotors.Application.Validators.Clientes;

using FluentValidation;
using FollowMotors.Application.Commands.Clientes;

/// <summary>Verifica los datos de entrada de DesactivarCuentaClienteCommand antes de ejecutar el Handler.</summary>
public sealed class DesactivarCuentaClienteValidator : AbstractValidator<DesactivarCuentaClienteCommand>
{
    /// <summary>Declara las restricciones que deberá cumplir la solicitud.</summary>
    public DesactivarCuentaClienteValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0UL);
    }
}
