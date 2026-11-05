using System.Security.Claims;
using FollowMotors.Api.Application.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FollowMotors.Api.Controllers;

/// <summary>
/// Acceso del personal a funciones protegidas. Es una dependencia técnica de
/// US06 y US15; NO implica que toda la US01 esté acabada (falta gestión de cuentas).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IStaffAuthService authService) : ControllerBase
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
            return Unauthorized(new ProblemDetails
            {
                Title = "No se pudo iniciar sesión",
                Detail = "Credenciales inválidas o cuenta sin permisos.",
                Status = StatusCodes.Status401Unauthorized
            });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nombre)
        };
        claims.AddRange(user.Permisos.Select(p => new Claim("permission", p)));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(6)
            });

        // Nunca devolver contraseña, hash ni secretos en el JSON.
        return Ok(new { usuario = user.Nombre, permisos = user.Permisos });
    }

    /// <summary>Eliminar la sesión al terminar el trabajo en sucursal.</summary>
    [HttpPost("logout")]
    [Authorize]
    [Consumes("application/json")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    /// <summary>Consulta mínima de identidad para un frontend Angular.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        usuario = User.Identity?.Name,
        permisos = User.FindAll("permission").Select(c => c.Value).ToArray()
    });
}
