namespace FollowMotors.Application.Commands.Atenciones;

using MediatR;

/// <summary>Solicita registrar los hallazgos de una revisión asignada.</summary>
public sealed record RegistrarDiagnosticoCommand(ulong AtencionId, string Hallazgos) : IRequest<bool>;
