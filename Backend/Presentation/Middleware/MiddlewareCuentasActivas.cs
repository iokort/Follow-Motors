namespace FollowMotors.Presentation.Middleware;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FollowMotors.Application.Abstractions.Autenticacion;

/// <summary>Impide el uso de sesiones revocadas, vencidas o con una cuenta inactiva.</summary>
public sealed class SesionesActivasMiddleware(RequestDelegate siguiente)
{
    /// <summary>Comprueba la sesión firmada antes de autorizar acceso a cualquier recurso privado.</summary>
    public async Task InvokeAsync(HttpContext contexto, IRepositorioSesiones sesiones)
    {
        if (contexto.User.Identity?.IsAuthenticated == true)
        {
            contexto.Response.Headers.CacheControl = "no-store";
            string? sesion = contexto.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            string? rol = contexto.User.FindFirstValue(ClaimTypes.Role);
            if (!ulong.TryParse(contexto.User.FindFirstValue(ClaimTypes.NameIdentifier), out ulong id)
                || !Guid.TryParseExact(sesion, "N", out _) || rol is null
                || !await sesiones.EsValidaAsync(id, rol, sesion!, contexto.RequestAborted))
            {
                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }
        await siguiente(contexto);
    }
}
