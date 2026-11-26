namespace FollowMotors.Presentation.Controllers.Gerencia;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FollowMotors.Application.Queries.Gerencia;

/// <summary>Presenta las funciones exclusivas del gerente sin lógica de negocio.</summary>
[ApiController]
[Route("api/gerencia")]
[Authorize(Roles = "GERENTE")]
public sealed class GerenciaController(ISender mediador) : ControllerBase
{
    /// <summary>Consulta el perfil del gerente actual mediante CQRS y MediatR.</summary>
    [HttpGet("mi-perfil")]
    public async Task<IActionResult> MiPerfil(CancellationToken ct) =>
        Ok(await mediador.Send(new ConsultarPerfilGerenteQuery(), ct));
}
