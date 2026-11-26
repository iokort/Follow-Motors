namespace FollowMotors.Domain.Entities.Atenciones;

/// <summary>Contiene el pago único de la tarifa fija de una revisión.</summary>
public sealed record CobroRevision(ulong Id, ulong AtencionId, ulong TarifaId,
    decimal ImporteCobrado, ulong RegistradoPorUsuarioId, DateTime CobradoEn,
    string? ReferenciaComprobante);
