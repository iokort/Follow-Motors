namespace FollowMotors.Presentation.Controllers.Motocicletas;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FollowMotors.Application.Commands.Motocicletas;
using FollowMotors.Application.Queries.Motocicletas;

/// <summary>Expone el registro interno y la consulta privada de motocicletas.</summary>
[ApiController]
[Route("api/motocicletas")]
[Authorize]
public sealed class MotocicletasController(ISender mediador) : ControllerBase
{
    /// <summary>Solicita que el personal vincule una motocicleta a su propietario.</summary>
    [HttpPost]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarMotocicletaCommand solicitud,
        CancellationToken ct)
    {
        ulong id = await mediador.Send(solicitud, ct);
        return Created($"/api/motocicletas/{id}", new { id });
    }

    /// <summary>Devuelve solamente las unidades del cliente autenticado.</summary>
    [HttpGet("mis-motocicletas")]
    [Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> MisMotocicletas(CancellationToken ct) =>
        Ok(await mediador.Send(new ConsultarMisMotocicletasQuery(), ct));
}
