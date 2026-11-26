namespace FollowMotors.Application.Queries.Atenciones;

using MediatR;
using FollowMotors.Domain.Entities.Atenciones;

/// <summary>Solicita consultar una atención con verificación de autorización.</summary>
public sealed record ConsultarAtencionQuery(ulong AtencionId) : IRequest<AtencionTaller?>;
