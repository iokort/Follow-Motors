namespace FollowMotors.Application.Queries.Clientes;

using MediatR;
using FollowMotors.Application.Models.Clientes;

/// <summary>Consulta el expediente privado de la cuenta cliente autenticada.</summary>
public sealed record ConsultarExpedientePropioQuery() : IRequest<ExpedientePropio>;

/// <summary>Consulta una página de expedientes por prefijo de identificación o nombre.</summary>
public sealed record BuscarClientesQuery(string? Buscar, int Pagina = 1)
    : IRequest<IReadOnlyList<ClienteFicha>>;

/// <summary>Recupera un expediente para el personal autorizado.</summary>
public sealed record ObtenerClienteQuery(ulong Id) : IRequest<ClienteFicha>;
