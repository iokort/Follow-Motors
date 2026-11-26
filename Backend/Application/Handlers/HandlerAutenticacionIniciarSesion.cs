namespace FollowMotors.Application.Handlers.Autenticacion;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Application.Abstractions.Repositories.Clientes;
using FollowMotors.Application.Commands.Autenticacion;
using FollowMotors.Application.Common.Excepciones;
using FollowMotors.Application.Models.Autenticacion;
using MediatR;

/// <summary>Gestiona el inicio y cierre de sesión con credenciales protegidas.</summary>
public sealed class SesionesHandler(IRepositorioCuentas cuentas, IProtectorContrasenas contrasenas,
    IRepositorioSesiones sesiones, IGeneradorTokens tokens, IUsuarioActual usuario, TimeProvider reloj)
    : IRequestHandler<IniciarSesionCommand, SesionCliente>, IRequestHandler<CerrarSesionCommand>
{
    /// <summary>Comprueba la cuenta y persiste la sesión antes de entregar el token.</summary>
    /// <exception cref="CredencialesInvalidasException">Las credenciales o la cuenta no son válidas.</exception>
    public async Task<SesionCliente> Handle(IniciarSesionCommand solicitud, CancellationToken ct)
    {
        var cuenta = await cuentas.BuscarPorIdentificadorAsync(solicitud.Identificador.Trim().ToLowerInvariant(), ct);
        if (cuenta is null || !cuenta.Activo || !contrasenas.Verificar(solicitud.Contrasena, cuenta.HashContrasena))
            throw new CredencialesInvalidasException();

        string sesionId = Guid.NewGuid().ToString("N");
        var expiraEn = reloj.GetUtcNow().UtcDateTime.AddHours(2);
        string token = tokens.Generar(cuenta.Id, cuenta.Identificador, cuenta.Rol, sesionId, expiraEn);
        if (!await sesiones.CrearAsync(cuenta.Id, cuenta.Rol, sesionId, expiraEn, ct))
            throw new CredencialesInvalidasException();
        return new SesionCliente(cuenta.Id, cuenta.Identificador, cuenta.Rol, token, expiraEn);
    }

    /// <summary>Revoca el identificador firmado de la sesión que ejecuta el cierre.</summary>
    public Task Handle(CerrarSesionCommand solicitud, CancellationToken ct)
    {
        if (usuario.UsuarioId is not ulong id || usuario.SesionId is not string sesion)
            throw new UnauthorizedAccessException("Se requiere una sesión válida.");
        return sesiones.RevocarAsync(id, sesion, ct);
    }
}
