namespace FollowMotors.Presentation.Controllers.Catalogos;

using FollowMotors.Application.Queries.Catalogos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>Expone la proyección operativa para el personal.</summary>
[ApiController]
[Authorize(Roles = "GERENTE,EMPLEADO")]
public sealed class CatalogoController(ISender mediador) : ControllerBase
{
    /// <summary>Responde con datos MySQL y acepta la ruta histórica que utiliza Angular.</summary>
    [HttpGet("api/taller/catalogos")]
    [HttpGet("api/demo/catalogos")]
    public async Task<IActionResult> Catalogos(CancellationToken ct) =>
        Ok(await mediador.Send(new ConsultarCatalogoTallerQuery(), ct));
}
