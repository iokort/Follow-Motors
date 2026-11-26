namespace FollowMotors.Application.Queries.Demostracion;

using FollowMotors.Application.Models.Demostracion;
using MediatR;

/// <summary>Consulta las cuentas ficticias habilitadas solo para la demostración local.</summary>
public sealed record ConsultarAccesosDemostracionQuery() : IRequest<IReadOnlyList<AccesoDemostracion>>;

/// <summary>Consulta únicamente los catálogos ficticios para mostrar los flujos existentes.</summary>
public sealed record ConsultarCatalogosDemostracionQuery() : IRequest<ResumenDemostracion>;
