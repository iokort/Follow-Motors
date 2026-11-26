namespace FollowMotors.Infrastructure.Autenticacion;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Models.Autenticacion;
using FollowMotors.Application.Common.Excepciones;

/// <summary>Servicio de autenticación de usuarios activos por identificador y contraseña.</summary>
public sealed class ServicioAutenticacion(IRepositorioCuentas cuentas,
    IProtectorContrasenas contrasenas, IGeneradorTokens tokens) : IServicioAutenticacion
{
    /// <summary>Rechaza credenciales inválidas o cuentas desactivadas sin revelar el motivo.</summary>
    public async Task<SesionCliente> IniciarSesionAsync(string identificador, string contrasena,
        CancellationToken ct)
    {
        var cuenta = await cuentas.BuscarPorIdentificadorAsync(identificador.Trim().ToLowerInvariant(), ct);
        if (cuenta is null || !cuenta.PuedeIniciarSesion() ||
            !contrasenas.Verificar(contrasena, cuenta.HashContrasena))
            throw new CredencialesInvalidasException();
        return new SesionCliente(cuenta.Id, cuenta.Identificador, cuenta.Rol,
            tokens.Generar(cuenta.Id, cuenta.Identificador, cuenta.Rol), DateTime.UtcNow.AddHours(2));
    }
}
