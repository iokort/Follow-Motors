namespace FollowMotors.Application.Queries.Gerencia;

using MediatR;
using FollowMotors.Application.Models.Gerencia;

/// <summary>Solicita el perfil del gerente autenticado, sin recibir identificadores externos.</summary>
public sealed record ConsultarPerfilGerenteQuery() : IRequest<PerfilGerente>;
