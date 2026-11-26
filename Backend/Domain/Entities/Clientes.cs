namespace FollowMotors.Domain.Entities.Clientes;

/// <summary>Representa los datos personales del propietario de una motocicleta.</summary>
public sealed record Cliente(ulong Id, ulong UsuarioId, string Nombres, string Apellidos,
    string? Telefono, ulong? RegistradoPorUsuarioId, DateTime CreadoEn)
{
    /// <summary>Devuelve el nombre del cliente sin modificar sus datos originales.</summary>
    public string ObtenerNombreCompleto() => $"{Nombres} {Apellidos}".Trim();
}

/// <summary>Representa una cuenta de acceso, incluido su rol y su estado vigente.</summary>
public sealed record CuentaCliente(ulong Id, string Identificador, string HashContrasena,
    string Rol, bool Activo, DateTime CreadoEn)
{
    /// <summary>Indica si la cuenta puede autenticarse según su estado.</summary>
    public bool PuedeIniciarSesion() => Activo;
}
