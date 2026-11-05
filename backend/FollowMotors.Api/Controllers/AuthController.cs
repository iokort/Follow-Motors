using System.Security.Claims;
using FollowMotors.Api.Application.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FollowMotors.Api.Controllers;

/// <summary>
/// US01: autenticación REAL del gerente/asesor desde el esquema MySQL V3.
/// Se reutilizan las mismas rutas de sesión para US06 y US15.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IStaffAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Sesión de personal con cookie HttpOnly, SameSite Strict y expiración;
    /// solo autentica cuentas existentes en el esquema de Gael.
    /// </summary>
    [HttpPost("staff-login")]
    [Consumes("application/json")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] StaffLoginRequest request, CancellationToken ct)
    {
        var user = await authService.LoginAsync(request, ct);
        if (user is null)
        {
            // CRC-US01-12: no registrar el correo ni el secreto en la bitácora.
            logger.LogWarning("US01: intento de inicio de sesión rechazado");
            return Unauthorized(new ProblemDetails
            {
                Title = "No se pudo iniciar sesión",
                Detail = "Credenciales inválidas o cuenta sin permisos.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nombre)
        };
        claims.AddRange(user.Permisos.Select(p => new Claim("permission", p)));
        // Rol obtenido de usuarios_roles y roles activos; no del navegador.
        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(6)
            });

        logger.LogInformation("US01: sesión de personal creada");
        // Nunca devolver contraseña, hash ni secretos en el JSON.
        return Ok(new { usuario = user.Nombre, roles = user.Roles, permisos = user.Permisos });
    }

    /// <summary>Eliminar la sesión al terminar el trabajo en sucursal.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    /// <summary>
    /// US01: comprueba el permiso de entrar al área de GERENCIA.
    /// Devuelve 401 sin sesión, 403 para cuentas con otro rol y 200 al gerente.
    /// </summary>
    [HttpGet("gerencia/acceso")]
    [Authorize(Policy = "Gerencia")]
    public IActionResult VerificarAccesoGerencia() =>
        Ok(new { acceso = "GERENTE", usuario = User.Identity?.Name });

    /// <summary>Consulta mínima de identidad para un frontend Angular.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        usuario = User.Identity?.Name,
        roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray(),
        permisos = User.FindAll("permission").Select(c => c.Value).ToArray()
    });
}
