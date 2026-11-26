namespace FollowMotors.Application.Abstractions.Autenticacion;

using FollowMotors.Application.Models.Autenticacion;

/// <summary>Operación de inicio de sesión sin acceso directo a la infraestructura.</summary>
public interface IServicioAutenticacion
{
    /// <summary>Autentica credenciales de una cuenta activa y devuelve el acceso autorizado.</summary>
    Task<SesionCliente> IniciarSesionAsync(string identificador, string contrasena, CancellationToken ct);
}
