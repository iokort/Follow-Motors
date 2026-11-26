namespace FollowMotors.Presentation.Controllers.Demostracion;

using FollowMotors.Application.Queries.Demostracion;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>Expone solo en modo local los catálogos de apoyo de los flujos académicos.</summary>
[ApiController]
[Route("api/demo")]
[Authorize(Roles = "GERENTE,EMPLEADO")]
public sealed class DemostracionController(ISender mediador, IConfiguration configuracion, IWebHostEnvironment entorno)
    : ControllerBase
{
    /// <summary>Envía una Query, sin acceder a repositorios ni datos desde el controlador.</summary>
    [HttpGet("accesos")]
    [AllowAnonymous]
    public async Task<IActionResult> Accesos(CancellationToken ct)
    {
        if (!entorno.IsDevelopment() || !configuracion.GetValue<bool>("Demo:Activado")) return NotFound();
        return Ok(await mediador.Send(new ConsultarAccesosDemostracionQuery(), ct));
    }

    [HttpGet("catalogos")]
    public async Task<IActionResult> Catalogos(CancellationToken ct)
    {
        if (!entorno.IsDevelopment() || !configuracion.GetValue<bool>("Demo:Activado")) return NotFound();
        return Ok(await mediador.Send(new ConsultarCatalogosDemostracionQuery(), ct));
    }
}
