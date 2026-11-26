namespace FollowMotors.Domain.Rules.Atenciones;

using FollowMotors.Domain.Entities.Atenciones;

/// <summary>Contrato de reglas para estados de revisión.</summary>
public interface IPoliticaAtenciones
{
    /// <summary>Impide registrar un diagnóstico sobre una atención cerrada.</summary>
    void ExigirDiagnosticoPermitido(AtencionTaller atencion);
    /// <summary>Impide cobrar una revisión que todavía no fue diagnosticada.</summary>
    void ExigirCobroPermitido(AtencionTaller atencion);
}

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

/// <summary>Identifica a un mecánico elegible y el número de atenciones bajo su responsabilidad.</summary>
public sealed record CargaMecanico(ulong Id, long AtencionesAbiertas);

/// <summary>Elige al responsable con menor carga, con desempate estable por identificador.</summary>
public static class AsignacionMecanicos
{
    /// <summary>Devuelve el ID del personal menos ocupado o rechaza si el taller no tiene un mecánico disponible.</summary>
    public static ulong Elegir(IEnumerable<CargaMecanico> candidatos) =>
        candidatos.OrderBy(x => x.AtencionesAbiertas).ThenBy(x => x.Id).FirstOrDefault()?.Id
        ?? throw new InvalidOperationException("El taller no tiene mecánicos disponibles.");
}
