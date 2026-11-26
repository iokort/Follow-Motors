namespace FollowMotors.Presentation.Middleware;

using System.Security.Claims;
using FollowMotors.Application.Abstractions.Repositories.Clientes;

/// <summary>Comprueba que una cuenta continúe activa antes de ejecutar acciones privadas.</summary>
public sealed class CuentasActivasMiddleware(RequestDelegate siguiente)
{
    /// <summary>Revoca inmediatamente el acceso de cuentas desactivadas o con rol modificado.</summary>
    public async Task InvokeAsync(HttpContext contexto, IRepositorioCuentas cuentas)
    {
        if (contexto.User.Identity?.IsAuthenticated == true)
        {
            string? idTexto = contexto.User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? rol = contexto.User.FindFirstValue(ClaimTypes.Role);
            if (!ulong.TryParse(idTexto, out ulong id))
            {
                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            var cuenta = await cuentas.ObtenerPorIdAsync(id, contexto.RequestAborted);
            if (cuenta is null || !cuenta.Activo || cuenta.Rol != rol)
            {
                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }
        await siguiente(contexto);
    }
}
