namespace FollowMotors.Application.Queries.Personal;

using MediatR;
using FollowMotors.Domain.Entities.Personal;

/// <summary>Solicita una lista del personal sin datos de autenticación.</summary>
public sealed record ConsultarPersonalQuery() : IRequest<IReadOnlyList<Empleado>>;
