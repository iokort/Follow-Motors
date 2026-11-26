namespace FollowMotors.Application.Queries.Clientes;

using MediatR;
using FollowMotors.Application.Models.Clientes;

/// <summary>Consulta el expediente privado de la cuenta cliente autenticada.</summary>
public sealed record ConsultarExpedientePropioQuery() : IRequest<ExpedientePropio>;
