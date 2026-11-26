namespace FollowMotors.Application.Commands.Atenciones;

using MediatR;

/// <summary>Solicita cobrar la tarifa vigente sin aceptar importes arbitrarios del cliente.</summary>
public sealed record CobrarRevisionCommand(ulong AtencionId, string? ReferenciaComprobante) : IRequest<ulong>;

/// <summary>Solicita un ingreso con un único mecánico responsable.</summary>
public sealed record RecepcionarMotocicletaCommand(ulong ClienteId, ulong MotocicletaId,
    ulong MecanicoId, string Motivo, uint KilometrajeIngreso,
    ulong? CitaId) : IRequest<ulong>;

/// <summary>Solicita registrar los hallazgos de una revisión asignada.</summary>
public sealed record RegistrarDiagnosticoCommand(ulong AtencionId, string Hallazgos) : IRequest<bool>;
