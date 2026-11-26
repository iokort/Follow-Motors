namespace FollowMotors.Presentation.Autenticacion;

using System.Security.Claims;
using FollowMotors.Application.Abstractions.Autenticacion;

/// <summary>Adapta las claims verificadas de HTTP al contrato de identidad del caso de uso.</summary>
public sealed class UsuarioActualHttp(IHttpContextAccessor http) : IUsuarioActual
{
    public ulong? UsuarioId => ulong.TryParse(
        http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out ulong id) ? id : null;
    public string Rol => http.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ?? "";
}
