namespace FollowMotors.Application.Abstractions.Autenticacion;

using FollowMotors.Application.Models.Autenticacion;

/// <summary>Emite un token firmado para una identidad autenticada.</summary>
public interface IGeneradorTokens
{
    /// <summary>Genera el token sin introducir lógica de transporte en Application.</summary>
    string Generar(ulong usuarioId, string identificador, string rol);
}

/// <summary>Abstrae el almacenamiento seguro y la verificación de contraseñas.</summary>
public interface IProtectorContrasenas
{
    /// <summary>Devuelve un hash irreversible con sal individual.</summary>
    string GenerarHash(string contrasena);

    /// <summary>Comprueba la contraseña sin exponer el valor almacenado.</summary>
    bool Verificar(string contrasena, string hash);
}

/// <summary>Operación de inicio de sesión sin acceso directo a la infraestructura.</summary>
public interface IServicioAutenticacion
{
    /// <summary>Autentica credenciales de una cuenta activa y devuelve el acceso autorizado.</summary>
    Task<SesionCliente> IniciarSesionAsync(string identificador, string contrasena, CancellationToken ct);
}

/// <summary>Expone los datos mínimos del usuario autenticado al caso de uso.</summary>
public interface IUsuarioActual
{
    ulong? UsuarioId { get; }
    string Rol { get; }
}
