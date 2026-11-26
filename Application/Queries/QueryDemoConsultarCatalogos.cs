namespace FollowMotors.Application.Queries.Demostracion;

using FollowMotors.Application.Models.Demostracion;
using MediatR;

/// <summary>Consulta únicamente los catálogos ficticios para mostrar los flujos existentes.</summary>
public sealed record ConsultarCatalogosDemostracionQuery() : IRequest<ResumenDemostracion>;
