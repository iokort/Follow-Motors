namespace FollowMotors.Domain.Entities.Atenciones;

/// <summary>Describe el ingreso de una motocicleta y el avance de su revisión.</summary>
public sealed record AtencionTaller(ulong Id, ulong ClienteId, ulong MotocicletaId, ulong? CitaId,
    ulong MecanicoId, string Motivo, uint KilometrajeIngreso, string? TextoDiagnostico,
    string Estado, DateTime IngresadaEn, DateTime? DiagnosticadaEn)
{
    /// <summary>Indica si todavía está permitido registrar un diagnóstico inicial.</summary>
    public bool PermiteDiagnostico() => Estado == "RECIBIDA";
}

/// <summary>Contiene el pago único de la tarifa fija de una revisión.</summary>
public sealed record CobroRevision(ulong Id, ulong AtencionId, ulong TarifaId,
    decimal ImporteCobrado, ulong RegistradoPorUsuarioId, DateTime CobradoEn,
    string? ReferenciaComprobante);

/// <summary>Representa los hallazgos técnicos obtenidos durante la revisión.</summary>
public sealed record Diagnostico(ulong AtencionId, ulong MecanicoId, string Hallazgos, DateTime RegistradoEn);
