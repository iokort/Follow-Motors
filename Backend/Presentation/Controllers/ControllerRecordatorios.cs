namespace FollowMotors.Presentation.Controllers.Recordatorios;

using FollowMotors.Application.Operaciones;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>Gestiona el consentimiento y los avisos internos del cliente.</summary>
[ApiController]
[Route("api/recordatorios")]
[Authorize(Roles = "CLIENTE")]
public sealed class RecordatoriosController(ISender mediador) : ControllerBase
{
    /// <summary>Devuelve los recordatorios válidos para la cuenta actual.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct) =>
        Ok(await mediador.Send(new MisRecordatoriosQuery(), ct));

    /// <summary>Consulta si el titular habilitó los recordatorios.</summary>
    [HttpGet("preferencia")]
    public async Task<IActionResult> Preferencia(CancellationToken ct) =>
        Ok(await mediador.Send(new PreferenciaRecordatoriosQuery(), ct));

    /// <summary>Acepta la elección de consentimiento para el canal interno.</summary>
    [HttpPut("preferencia")]
    public async Task<IActionResult> Preferencia(CambiarPreferenciaRecordatoriosCommand solicitud, CancellationToken ct)
    {
        await mediador.Send(solicitud, ct);
        return NoContent();
    }
}
