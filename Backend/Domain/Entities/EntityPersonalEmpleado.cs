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
