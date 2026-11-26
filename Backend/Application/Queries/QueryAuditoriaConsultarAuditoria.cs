namespace FollowMotors.Application.Queries.Auditoria;

using MediatR;
using FollowMotors.Domain.Entities.Auditoria;

/// <summary>Solicita los últimos cambios auditados para consulta gerencial.</summary>
public sealed record ConsultarAuditoriaQuery(int Limite = 50) : IRequest<IReadOnlyList<RegistroAuditoria>>;
