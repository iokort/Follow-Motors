namespace FollowMotors.Application.Abstractions.Autenticacion;

/// <summary>Abstrae el almacenamiento seguro y la verificación de contraseñas.</summary>
public interface IProtectorContrasenas
{
    /// <summary>Devuelve un hash irreversible con sal individual.</summary>
    string GenerarHash(string contrasena);

    /// <summary>Comprueba la contraseña sin exponer el valor almacenado.</summary>
    bool Verificar(string contrasena, string hash);
}
