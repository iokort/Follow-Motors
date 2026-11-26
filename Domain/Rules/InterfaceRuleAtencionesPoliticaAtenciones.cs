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
