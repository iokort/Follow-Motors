namespace FollowMotors.Domain.Entities.Personal;

/// <summary>Representa al personal registrado y asociado a una cuenta de usuario.</summary>
public class Empleado
{
    public ulong Id { get; }
    public ulong UsuarioId { get; }
    public string Nombres { get; }
    public string Apellidos { get; }
    public bool EsMecanico { get; }
    public bool Disponible { get; }
    public string Rol { get; }

    /// <summary>Crea un perfil de personal a partir de sus datos persistidos.</summary>
    public Empleado(ulong id, ulong usuarioId, string nombres, string apellidos,
        bool esMecanico, bool disponible, string rol = "EMPLEADO")
    {
        Id = id;
        UsuarioId = usuarioId;
        Nombres = nombres;
        Apellidos = apellidos;
        EsMecanico = esMecanico;
        Disponible = disponible;
        Rol = rol;
    }

    /// <summary>Obtiene el nombre completo del empleado.</summary>
    public string ObtenerNombreCompleto() => $"{Nombres} {Apellidos}".Trim();
}

/// <summary>Representa a un empleado cuya cuenta tiene el rol GERENTE.</summary>
/// <remarks>El rol GERENTE se almacena en usuarios.rol; no necesita una tabla adicional.</remarks>
public sealed class Gerente : Empleado
{
    /// <summary>Construye la representación del gerente autenticado.</summary>
    public Gerente(ulong id, ulong usuarioId, string nombres, string apellidos, bool disponible)
        : base(id, usuarioId, nombres, apellidos, false, disponible, "GERENTE") { }
}

/// <summary>Identifica a un empleado habilitado para realizar diagnósticos técnicos.</summary>
public sealed class Mecanico : Empleado
{
    /// <summary>Construye un mecánico registrado en la plantilla del taller.</summary>
    public Mecanico(ulong id, ulong usuarioId, string nombres, string apellidos, bool disponible)
        : base(id, usuarioId, nombres, apellidos, true, disponible) { }
}
