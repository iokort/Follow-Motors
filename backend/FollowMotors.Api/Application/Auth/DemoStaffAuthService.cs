using Microsoft.AspNetCore.Identity;

namespace FollowMotors.Api.Application.Auth;

/// <summary>
/// US01: autenticación de UN gerente ficticio sin MySQL, solo para demostraciones.
/// Reutiliza IStaffAuthService: el controlador no sabe si los datos vienen de MySQL
/// o de esta cuenta temporal (inversión de dependencias, SOLID D).
/// NO registra usuarios, NO guarda contraseñas en el repositorio y NO se habilita
/// en Production; Program.cs selecciona este servicio solo en Development.
/// </summary>
public sealed class DemoStaffAuthService : IStaffAuthService
{
    // CRC-US01-01 y 02: datos mínimos del usuario y permisos del gerente.
    // Se usa un record privado porque no hace falta inventar otras 2 clases públicas.
    private sealed record CuentaDemo(Guid Id, string Nombre, string Correo, bool Activo, string Hash);

    // La demo habilita solamente los permisos que ya existen en el backend.
    // No se conceden permisos nuevos ni se implementa aquí US21.
    private static readonly string[] PermisosGerente =
        ["agenda.ver", "agenda.crear", "agenda.modificar"];

    private readonly CuentaDemo _cuenta;
    private readonly PasswordHasher<CuentaDemo> _hasher = new();
    private readonly ILogger<DemoStaffAuthService> _logger;

    public DemoStaffAuthService(IConfiguration config, ILogger<DemoStaffAuthService> logger)
    {
        _logger = logger;

        // CRC-US01-09: repositorio temporal de UNA cuenta, configurada al arrancar.
        // Los valores se inyectan mediante variables de entorno, nunca appsettings.
        var correo = config["Auth:Demo:Correo"]?.Trim();
        var contrasena = config["Auth:Demo:Contrasena"];
        if (string.IsNullOrWhiteSpace(correo) || correo.Length > 254 ||
            string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 8)
        {
            throw new InvalidOperationException(
                "La demo US01 requiere Auth__Demo__Correo y Auth__Demo__Contrasena (mínimo 8 caracteres).");
        }

        var nombre = config["Auth:Demo:Nombre"];
        if (string.IsNullOrWhiteSpace(nombre)) nombre = "Gerente de demostración";

        var cuenta = new CuentaDemo(
            Guid.NewGuid(), nombre, correo,
            config.GetValue("Auth:Demo:Activo", true), "");

        // CRC-US01-10: se genera un hash Identity PBKDF2 al iniciar el programa.
        // La contraseña no se conserva en _cuenta ni se escribe en los logs.
        _cuenta = cuenta with { Hash = _hasher.HashPassword(cuenta, contrasena) };
    }

    /// <summary>
    /// CRC-US01-08: verifica la cuenta, estado activo y hash; no genera cookies.
    /// La cookie y su caducidad pertenecen a AuthController/ASP.NET Core (SRP).
    /// La misma respuesta null oculta si falló el correo, la clave o el estado.
    /// </summary>
    public Task<StaffIdentity?> LoginAsync(StaffLoginRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var entradaValida = !string.IsNullOrWhiteSpace(request.Correo) &&
                            !string.IsNullOrWhiteSpace(request.Contrasena) &&
                            request.Correo.Length <= 254 &&
                            request.Contrasena.Length <= 500;

        var accesoValido = entradaValida &&
            _cuenta.Activo &&
            string.Equals(request.Correo.Trim(), _cuenta.Correo, StringComparison.OrdinalIgnoreCase) &&
            _hasher.VerifyHashedPassword(_cuenta, _cuenta.Hash, request.Contrasena)
                != PasswordVerificationResult.Failed;

        // CRC-US01-12: bitácora básica de la DEMO, sin email ni contraseña.
        // Una auditoría persistente de accesos queda para el backend definitivo.
        _logger.LogInformation("US01 (demo): acceso {Resultado}",
            accesoValido ? "autorizado" : "rechazado");

        StaffIdentity? usuario = accesoValido
            ? new StaffIdentity(_cuenta.Id, _cuenta.Nombre, PermisosGerente)
            : null;

        return Task.FromResult(usuario);
    }
}
