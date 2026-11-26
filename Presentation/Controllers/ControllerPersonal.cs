namespace FollowMotors.Presentation.Controllers.Personal;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FollowMotors.Application.Commands.Personal;
using FollowMotors.Application.Queries.Personal;

/// <summary>Administración exclusiva de personal desde la cuenta gerencial.</summary>
[ApiController]
[Route("api/personal")]
[Authorize(Roles = "GERENTE")]
public sealed class PersonalController(ISender mediador) : ControllerBase
{
    /// <summary>Registra un empleado o mecánico usando Commands y Validators.</summary>
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarEmpleadoCommand solicitud,
        CancellationToken ct)
    {
        ulong id = await mediador.Send(solicitud, ct);
        return Created($"/api/personal/{id}", new { id });
    }

    /// <summary>Devuelve la plantilla sin información de autenticación.</summary>
    [HttpGet]
    public async Task<IActionResult> Consultar(CancellationToken ct) =>
        Ok(await mediador.Send(new ConsultarPersonalQuery(), ct));
}
