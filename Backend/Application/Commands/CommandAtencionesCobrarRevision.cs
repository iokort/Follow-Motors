namespace FollowMotors.Application.Commands.Atenciones;

using MediatR;

/// <summary>Solicita cobrar la tarifa vigente sin aceptar importes arbitrarios del cliente.</summary>
public sealed record CobrarRevisionCommand(ulong AtencionId, string? ReferenciaComprobante) : IRequest<ulong>;
