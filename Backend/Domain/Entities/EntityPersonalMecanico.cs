namespace FollowMotors.Domain.Entities.Personal;

/// <summary>Identifica a un empleado habilitado para realizar diagnósticos técnicos.</summary>
public sealed class Mecanico : Empleado
{
    /// <summary>Construye un mecánico registrado en la plantilla del taller.</summary>
    public Mecanico(ulong id, ulong usuarioId, string nombres, string apellidos, bool disponible)
        : base(id, usuarioId, nombres, apellidos, true, disponible) { }
}
