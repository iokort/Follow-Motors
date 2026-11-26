namespace FollowMotors.Application.Queries.Motocicletas;

using MediatR;
using FollowMotors.Domain.Entities.Motocicletas;

/// <summary>Consulta únicamente las motos del cliente autenticado.</summary>
public sealed record ConsultarMisMotocicletasQuery() : IRequest<IReadOnlyList<Motocicleta>>;
