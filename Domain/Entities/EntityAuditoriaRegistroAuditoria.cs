namespace FollowMotors.Domain.Entities.Auditoria;

/// <summary>Contiene la trazabilidad de un cambio realizado sobre un registro.</summary>
public sealed record RegistroAuditoria(ulong Id, ulong? UsuarioId, string Entidad,
    ulong RegistroId, string Accion, string? Antes, string? Despues, DateTime RegistradaEn);
