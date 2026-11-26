namespace FollowMotors.Domain.Entities.Atenciones;

/// <summary>Describe el ingreso de una motocicleta y el avance de su revisión.</summary>
public sealed record AtencionTaller(ulong Id, ulong ClienteId, ulong MotocicletaId, ulong? CitaId,
    ulong MecanicoId, string Motivo, uint KilometrajeIngreso, string? TextoDiagnostico,
    string Estado, DateTime IngresadaEn, DateTime? DiagnosticadaEn)
{
    /// <summary>Indica si todavía está permitido registrar un diagnóstico inicial.</summary>
    public bool PermiteDiagnostico() => Estado == "RECIBIDA";
}
