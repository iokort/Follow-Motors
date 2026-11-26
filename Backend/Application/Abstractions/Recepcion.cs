namespace FollowMotors.Application.Abstractions.Operaciones;

using FollowMotors.Application.Commands.Atenciones;

/// <summary>Contrato que reúne los casos de uso del ingreso y revisión de motos.</summary>
public interface IRecepcionTaller
{
    /// <summary>Registra una atención presencial vinculada a una motocicleta.</summary>
    Task<ulong> RecibirAsync(RecepcionarMotocicletaCommand solicitud, CancellationToken ct);

    /// <summary>Permite al mecánico asignado documentar los hallazgos.</summary>
    Task<bool> DiagnosticarAsync(RegistrarDiagnosticoCommand solicitud, CancellationToken ct);

    /// <summary>Registra exactamente un cobro de diagnóstico por atención.</summary>
    Task<ulong> CobrarAsync(CobrarRevisionCommand solicitud, CancellationToken ct);
}
