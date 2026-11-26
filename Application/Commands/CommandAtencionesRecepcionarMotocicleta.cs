namespace FollowMotors.Application.Commands.Atenciones;

using MediatR;

/// <summary>Solicita un ingreso con un único mecánico responsable.</summary>
public sealed record RecepcionarMotocicletaCommand(ulong ClienteId, ulong MotocicletaId,
    ulong MecanicoId, string Motivo, uint KilometrajeIngreso,
    ulong? CitaId) : IRequest<ulong>;
