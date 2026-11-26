namespace FollowMotors.Application.Abstractions.Repositories.Atenciones;

using FollowMotors.Domain.Entities.Atenciones;

/// <summary>Contrato de persistencia para el ingreso, diagnóstico y pago de revisión.</summary>
public interface IRepositorioAtenciones
{
    /// <summary>Registra el ingreso comprobando propiedad y mecánico asignado.</summary>
    Task<ulong> CrearAsync(AtencionTaller atencion, ulong usuarioActorId, CancellationToken ct);

    /// <summary>Obtiene una atención por su identificador.</summary>
    Task<AtencionTaller?> ObtenerAsync(ulong atencionId, CancellationToken ct);

    /// <summary>Guarda el diagnóstico inicial y audita la transición de estado.</summary>
    Task<bool> RegistrarDiagnosticoAsync(ulong atencionId, string hallazgos,
        ulong usuarioActorId, CancellationToken ct);

    /// <summary>Cobra una sola vez la revisión con la tarifa vigente en base de datos.</summary>
    Task<ulong> CobrarRevisionAsync(ulong atencionId, ulong usuarioActorId,
        string? referenciaComprobante, CancellationToken ct);

    /// <summary>Finaliza una revisión pagada sin contrato.</summary>
    Task CerrarSinReparacionAsync(ulong atencionId, ulong usuarioActorId, CancellationToken ct);

    /// <summary>Modifica la tarifa vigente sin cambiar los cobros registrados.</summary>
    Task<ulong> ConfigurarTarifaAsync(decimal importe, ulong actorId, CancellationToken ct);
}
