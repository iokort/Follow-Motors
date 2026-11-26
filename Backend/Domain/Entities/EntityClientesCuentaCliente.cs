namespace FollowMotors.Domain.Entities.Clientes;

/// <summary>Representa una cuenta de acceso, incluido su rol y su estado vigente.</summary>
public sealed record CuentaCliente(ulong Id, string Identificador, string HashContrasena,
    string Rol, bool Activo, DateTime CreadoEn)
{
    /// <summary>Indica si la cuenta puede autenticarse según su estado.</summary>
    public bool PuedeIniciarSesion() => Activo;
}
