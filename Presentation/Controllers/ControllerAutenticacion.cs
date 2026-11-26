namespace FollowMotors.Presentation.Controllers.Autenticacion;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FollowMotors.Application.Commands.Autenticacion;

/// <summary>Exposición HTTP del inicio de sesión de empleados, gerente y clientes.</summary>
[ApiController]
[Route("api/autenticacion")]
public sealed class AutenticacionController(ISender mediador) : ControllerBase
{
    /// <summary>Recibe credenciales y delega la autenticación al Handler.</summary>
    [AllowAnonymous]
    [HttpPost("iniciar-sesion")]
    public async Task<IActionResult> IniciarSesion([FromBody] IniciarSesionCommand solicitud,
        CancellationToken ct) => Ok(await mediador.Send(solicitud, ct));
}
