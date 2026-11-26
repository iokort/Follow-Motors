namespace FollowMotors.Domain.Rules.Agenda;

/// <summary>Establece cuándo es posible reservar, cerrar o modificar una franja de revisión.</summary>
public static class ControlCapacidad
{
    /// <summary>Indica si la franja admite otra cita sin superar sus cupos.</summary>
    public static bool HayDisponibilidad(int capacidad, long reservasConfirmadas) =>
        capacidad > 0 && reservasConfirmadas >= 0 && reservasConfirmadas < capacidad;

    /// <summary>Protege el horario y cupo de citas confirmadas contra cambios incompatibles.</summary>
    /// <exception cref="InvalidOperationException">El nuevo horario o cupo afecta a citas confirmadas.</exception>
    public static void ValidarCambio(int capacidad, long reservasConfirmadas, bool conservaHorarioAbierto)
    {
        if (capacidad < 1 || reservasConfirmadas > capacidad)
            throw new InvalidOperationException("El cupo sería menor al número de citas confirmadas.");
        if (reservasConfirmadas > 0 && !conservaHorarioAbierto)
            throw new InvalidOperationException("Reprograma o cancela las citas confirmadas antes de cerrar o mover la franja.");
    }
}
