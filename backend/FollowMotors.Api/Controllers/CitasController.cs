using System.Security.Claims;
using FollowMotors.Api.Application.Agenda;
using FollowMotors.Api.Domain.Agenda;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FollowMotors.Api.Controllers;

/// <summary>
/// Controlador HTTP dedicado a las historias US06 y US15.
/// Solo convierte solicitudes en llamadas de aplicación y respuestas HTTP:
/// NO contiene SQL, reglas de negocio ni determina permisos por HTML (SRP).
/// </summary>
[ApiController]
[Route("api/v1/citas")]
[Authorize]
public sealed class CitasController(IAgendaService agenda, AgendaPolicy policy) : ControllerBase
{
    /// <summary>US15 — Agenda diaria: únicamente quien tenga agenda.ver.</summary>
    [HttpGet]
    [Authorize(Policy = "AgendaVer")]
    public async Task<ActionResult<IReadOnlyList<Cita>>> Listar([FromQuery] DateOnly? fecha, CancellationToken ct)
    {
        var dia = fecha ?? DateOnly.FromDateTime(policy.AhoraLocal);
        var citas = await agenda.ListarDiaAsync(dia, ct);
        return Ok(citas);
    }

    /// <summary>US15 — Consultar una cita existente con permiso de agenda.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AgendaVer")]
    public async Task<ActionResult<Cita>> Detalle(Guid id, CancellationToken ct)
    {
        var cita = await agenda.BuscarAsync(id, ct);
        return cita is null ? NotFound() : Ok(cita);
    }

    /// <summary>
    /// US06 — El personal registra cita CFMOTO o EXTERNA.
    /// Un cliente nunca puede obtener agenda.crear.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [Authorize(Policy = "AgendaCrear")]
    public async Task<ActionResult<Cita>> Crear([FromBody] CrearCitaRequest request, CancellationToken ct)
    {
        if (!TryActor(out var actor)) return Unauthorized();
        try
        {
            var cita = await agenda.CrearAsync(request, actor, ct);
            return CreatedAtAction(nameof(Detalle), new { id = cita.Id }, cita);
        }
        catch (AgendaException ex) { return Error(ex); }
    }

    /// <summary>US15 — Reprogramar sin borrar ni reinsertar la cita.</summary>
    [HttpPut("{id:guid}/reprogramar")]
    [Consumes("application/json")]
    [Authorize(Policy = "AgendaModificar")]
    public async Task<ActionResult<Cita>> Reprogramar(
        Guid id, [FromBody] ReprogramarCitaRequest request, CancellationToken ct)
    {
        if (!TryActor(out var actor)) return Unauthorized();
        try
        {
            var cita = await agenda.ReprogramarAsync(id, request, actor, ct);
            return cita is null ? NotFound() : Ok(cita);
        }
        catch (AgendaException ex) { return Error(ex); }
    }

    /// <summary>US15 — Cancelación lógica con motivo auditado.</summary>
    [HttpPut("{id:guid}/cancelar")]
    [Consumes("application/json")]
    [Authorize(Policy = "AgendaModificar")]
    public async Task<ActionResult<Cita>> Cancelar(
        Guid id, [FromBody] CancelarCitaRequest request, CancellationToken ct)
    {
        if (!TryActor(out var actor)) return Unauthorized();
        try
        {
            var cita = await agenda.CancelarAsync(id, request, actor, ct);
            return cita is null ? NotFound() : Ok(cita);
        }
        catch (AgendaException ex) { return Error(ex); }
    }

    /// <summary>
    /// Actor siempre procede de la identidad autenticada. Ignorar cualquier
    /// supuesta propiedad registradaPor que envíe el frontend.
    /// </summary>
    private bool TryActor(out Guid actor) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out actor) && actor != Guid.Empty;

    private ObjectResult Error(AgendaException ex) => Problem(
        title: "La operación de agenda no pudo completarse",
        detail: ex.Message, statusCode: ex.StatusCode);
}
