namespace FollowMotors.Application.Queries.Motocicletas;

using MediatR;
using FollowMotors.Domain.Entities.Motocicletas;

/// <summary>Consulta únicamente las motos del cliente autenticado.</summary>
public sealed record ConsultarMisMotocicletasQuery() : IRequest<IReadOnlyList<Motocicleta>>;

/// <summary>Consulta una unidad, sujeta a verificación de propietario.</summary>
public sealed record ObtenerMotocicletaQuery(ulong Id) : IRequest<Motocicleta>;

/// <summary>Busca fichas de unidades por prefijo de VIN, modelo o marca.</summary>
public sealed record BuscarMotocicletasQuery(string? Buscar, int Pagina = 1)
    : IRequest<IReadOnlyList<Motocicleta>>;
