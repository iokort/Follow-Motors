namespace FollowMotors.Application.Commands.Motocicletas;

using MediatR;

/// <summary>Solicita registrar una unidad multimarca con propietario verificado.</summary>
public sealed record RegistrarMotocicletaCommand(ulong ClienteId, string NumeroSerie,
    string Marca, string Modelo, int? Anio, string? Placa, string? Color,
    uint KilometrajeActual) : IRequest<ulong>;
