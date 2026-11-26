namespace FollowMotors.Application.Validators.Atenciones;

using FluentValidation;
using FollowMotors.Application.Commands.Atenciones;

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
