namespace FollowMotors.Application.Abstractions.Autenticacion;

/// <summary>Firma un token con la identidad, el identificador de sesión y su vencimiento.</summary>
public interface IGeneradorTokens
{
    /// <summary>Genera el token correspondiente a una sesión persistida.</summary>
    string Generar(ulong usuarioId, string identificador, string rol, string sesionId, DateTime expiraEn);
}

/// <summary>Protege y verifica contraseñas mediante hashes con sal individual.</summary>
public interface IProtectorContrasenas
{
    /// <summary>Calcula un hash irreversible con una sal nueva.</summary>
    string GenerarHash(string contrasena);

    /// <summary>Verifica la contraseña contra el hash almacenado.</summary>
    bool Verificar(string contrasena, string hash);
}

/// <summary>Expone la identidad verificada al caso de uso.</summary>
public interface IUsuarioActual
{
    ulong? UsuarioId { get; }
    string Rol { get; }
    string? SesionId { get; }
}

/// <summary>Conserva las sesiones activas para comprobarlas y revocarlas individualmente.</summary>
public interface IRepositorioSesiones
{
    /// <summary>Activa una sesión si la cuenta y el rol siguen vigentes.</summary>
    Task<bool> CrearAsync(ulong usuarioId, string rol, string sesionId, DateTime expiraEn, CancellationToken ct);

    /// <summary>Comprueba vigencia, titular y rol actual sin consultar hashes de contraseña.</summary>
    Task<bool> EsValidaAsync(ulong usuarioId, string rol, string sesionId, CancellationToken ct);

    /// <summary>Elimina la sesión del usuario; repetir el cierre conserva el mismo estado.</summary>
    Task RevocarAsync(ulong usuarioId, string sesionId, CancellationToken ct);
}
