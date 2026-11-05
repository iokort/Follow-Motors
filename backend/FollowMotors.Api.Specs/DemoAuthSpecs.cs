using FollowMotors.Api.Application.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Pruebas de US01 sin MySQL ni servidor: verifican el caso de uso y sus
/// resultados con datos completamente ficticios.
/// El controlador HTTP y la cookie se reutilizan sin cambios.
/// </summary>
internal static class DemoAuthSpecs
{
    public static async Task RunAsync()
    {
        const string correo = "gerente@example.invalid";
        const string clave = "Clave-Solo-Prueba-123";

        var servicio = CrearServicio(correo, clave, activo: true);

        // Caso feliz: la cuenta activa obtiene identidad y permisos limitados.
        var usuario = await servicio.LoginAsync(new StaffLoginRequest(correo, clave), default);
        Check(usuario is not null, "US01: credenciales válidas autentican al gerente.");
        Check(usuario!.Permisos.Contains("agenda.ver") &&
              usuario.Permisos.Contains("agenda.crear") &&
              usuario.Permisos.Contains("agenda.modificar"),
            "US01: se devuelven exclusivamente los permisos de agenda disponibles.");
        Check(usuario.Permisos.Count == 3, "US01: la demo no concede otros permisos.");

        // Casos de rechazo: nunca retorna una identidad que abra una sesión.
        Check(await servicio.LoginAsync(new StaffLoginRequest(correo, "incorrecta"), default) is null,
            "US01: contraseña incorrecta no autentica.");
        Check(await servicio.LoginAsync(new StaffLoginRequest("otro@example.invalid", clave), default) is null,
            "US01: cuenta inexistente no autentica.");
        Check(await servicio.LoginAsync(new StaffLoginRequest(" ", clave), default) is null,
            "US01: correo vacío no autentica.");
        Check(await servicio.LoginAsync(new StaffLoginRequest(correo, ""), default) is null,
            "US01: contraseña vacía no autentica.");

        var suspendida = CrearServicio(correo, clave, activo: false);
        Check(await suspendida.LoginAsync(new StaffLoginRequest(correo, clave), default) is null,
            "US01: cuenta inactiva no autentica.");

        // No se debe arrancar una demo accidentalmente sin clave configurada.
        try
        {
            CrearServicio(correo, "", activo: true);
            throw new Exception("US01: se permitió una demo sin contraseña.");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("OK: US01 requiere contraseña de demostración.");
        }
    }

    private static DemoStaffAuthService CrearServicio(string correo, string clave, bool activo)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Demo:Correo"] = correo,
                ["Auth:Demo:Contrasena"] = clave,
                ["Auth:Demo:Nombre"] = "Gerente de prueba",
                ["Auth:Demo:Activo"] = activo.ToString()
            }).Build();

        return new DemoStaffAuthService(config, NullLogger<DemoStaffAuthService>.Instance);
    }

    private static void Check(bool ok, string descripcion)
    {
        if (!ok) throw new Exception("Prueba fallida: " + descripcion);
        Console.WriteLine("OK: " + descripcion);
    }
}
