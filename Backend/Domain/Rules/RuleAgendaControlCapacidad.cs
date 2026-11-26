namespace FollowMotors.Domain.Rules.Agenda;

/// <summary>Protege los cupos de agenda frente a reservas adicionales.</summary>
public sealed class ControlCapacidad
{
    /// <summary>Indica si aún se permite una nueva reserva sin rebasar el cupo.</summary>
    public bool HayDisponibilidad(int capacidad, int reservasConfirmadas) =>
        capacidad > 0 && reservasConfirmadas >= 0 && reservasConfirmadas < capacidad;

    /// <summary>Comprueba que una reducción de cupos no deje más reservas que plazas.</summary>
    public void ValidarCambioCapacidad(int nuevaCapacidad, int reservasConfirmadas)
    {
        if (nuevaCapacidad < 1 || reservasConfirmadas > nuevaCapacidad)
            throw new InvalidOperationException("La capacidad no puede ser menor que las citas existentes.");
    }
}
