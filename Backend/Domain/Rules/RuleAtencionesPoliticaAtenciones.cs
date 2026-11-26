namespace FollowMotors.Domain.Rules.Atenciones;

using FollowMotors.Domain.Entities.Atenciones;

/// <summary>Resguarda las transiciones de estado de una revisión.</summary>
public sealed class PoliticaAtenciones : IPoliticaAtenciones
{
    /// <summary>Comprueba que la atención siga abierta para diagnosticarla.</summary>
    public void ExigirDiagnosticoPermitido(AtencionTaller atencion)
    {
        if (!atencion.PermiteDiagnostico())
            throw new InvalidOperationException("La atención ya fue diagnosticada o cerrada.");
    }

    /// <summary>Verifica que la revisión esté diagnosticada antes de cobrarla.</summary>
    public void ExigirCobroPermitido(AtencionTaller atencion)
    {
        if (atencion.Estado != "DIAGNOSTICADA")
            throw new InvalidOperationException("La revisión debe estar diagnosticada antes de cobrar.");
    }
}
