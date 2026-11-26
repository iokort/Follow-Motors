namespace FollowMotors.Domain.Entities.Personal;

/// <summary>Representa a un empleado cuya cuenta tiene el rol GERENTE.</summary>
/// <remarks>El rol GERENTE se almacena en usuarios.rol; no necesita una tabla adicional.</remarks>
public sealed class Gerente : Empleado
{
    /// <summary>Construye la representación del gerente autenticado.</summary>
    public Gerente(ulong id, ulong usuarioId, string nombres, string apellidos, bool disponible)
        : base(id, usuarioId, nombres, apellidos, false, disponible, "GERENTE") { }
}
