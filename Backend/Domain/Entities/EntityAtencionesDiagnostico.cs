namespace FollowMotors.Domain.Entities.Atenciones;

/// <summary>Representa los hallazgos técnicos obtenidos durante la revisión.</summary>
public sealed record Diagnostico(ulong AtencionId, ulong MecanicoId, string Hallazgos, DateTime RegistradoEn);
