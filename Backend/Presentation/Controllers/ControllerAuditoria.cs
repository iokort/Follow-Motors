namespace FollowMotors.Presentation.Controllers.Auditoria;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FollowMotors.Application.Queries.Auditoria;

/// <summary>Consulta controlada de auditoría para el gerente.</summary>
[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = "GERENTE")]
public sealed class AuditoriaController(ISender mediador) : ControllerBase
{
    /// <summary>Obtiene un número limitado de registros sin consultar directamente MySQL.</summary>
    [HttpGet]
    public async Task<IActionResult> Consultar([FromQuery] int limite = 50,
        CancellationToken ct = default) =>
        Ok(await mediador.Send(new ConsultarAuditoriaQuery(limite), ct));
}
