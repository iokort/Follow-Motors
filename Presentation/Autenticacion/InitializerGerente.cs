namespace FollowMotors.Presentation.Inicializacion;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Clientes;

/// <summary>Permite crear una sola cuenta GERENTE mediante variables de entorno de arranque.</summary>
public sealed class InicializadorGerente(IConfiguration configuracion,
    IRepositorioCuentas cuentas, IProtectorContrasenas protector,
    ILogger<InicializadorGerente> registro)
{
    /// <summary>Omite el alta cuando no hay configuración o ya existe un gerente activo.</summary>
    public async Task CrearSiHaceFaltaAsync(CancellationToken ct)
    {
        var identificador = configuracion["Bootstrap:Gerente:Identificador"];
        var contrasena = configuracion["Bootstrap:Gerente:Contrasena"];
        var nombres = configuracion["Bootstrap:Gerente:Nombres"];
        var apellidos = configuracion["Bootstrap:Gerente:Apellidos"];
        if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrWhiteSpace(contrasena)) return;
        if (contrasena.Length < 12 || string.IsNullOrWhiteSpace(nombres) ||
            string.IsNullOrWhiteSpace(apellidos))
            throw new InvalidOperationException("La configuración inicial de gerente está incompleta o es insegura.");
        if (await cuentas.ExisteGerenteAsync(ct)) return;
        bool creado = await cuentas.CrearGerenteInicialAsync(identificador.Trim().ToLowerInvariant(),
            protector.GenerarHash(contrasena), nombres.Trim(), apellidos.Trim(), ct);
        if (creado) registro.LogInformation("Se inicializó la cuenta gerencial.");
    }
}
