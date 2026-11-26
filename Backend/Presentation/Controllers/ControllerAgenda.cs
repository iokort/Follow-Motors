namespace FollowMotors.Presentation.Controllers.Agenda;

using FollowMotors.Application.Agenda;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>Administra franjas de revisión mediante los comandos y consultas de agenda.</summary>
[ApiController]
[Route("api/agenda/franjas")]
[Authorize(Roles = "GERENTE,EMPLEADO")]
public sealed class FranjasController(ISender mediador) : ControllerBase
{
    /// <summary>Devuelve franjas y cupos reales entre las fechas indicadas.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(DateTimeOffset desde, DateTimeOffset hasta,
        bool disponibles, CancellationToken ct) =>
        Ok(await mediador.Send(new FranjasQuery(desde, hasta, disponibles), ct));

    /// <summary>Da de alta una franja de recepción.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear(DatosFranja datos, CancellationToken ct) =>
        Ok(new { id = await mediador.Send(new GuardarFranjaCommand(null, datos), ct) });

    /// <summary>Envía los cambios autorizados al horario o capacidad de una franja.</summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Editar(ulong id, DatosFranja datos, CancellationToken ct) =>
        Ok(new { id = await mediador.Send(new GuardarFranjaCommand(id, datos), ct) });
}

/// <summary>Expone la agenda interna y las consultas de citas propias.</summary>
[ApiController]
[Route("api/citas")]
[Authorize]
public sealed class CitasController(ISender mediador) : ControllerBase
{
    /// <summary>Obtiene las citas visibles por rango y página.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(DateTimeOffset? desde, DateTimeOffset? hasta,
        int pagina = 1, CancellationToken ct = default) =>
        Ok(await mediador.Send(new CitasQuery(desde, hasta, pagina), ct));

    /// <summary>Obtiene el detalle autorizado de una cita.</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Obtener(ulong id, CancellationToken ct) =>
        Ok(await mediador.Send(new CitaQuery(id), ct));

    /// <summary>Registra una cita gestionada por personal del taller.</summary>
    [HttpPost]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Registrar(RegistrarCitaCommand solicitud, CancellationToken ct)
    {
        var id = await mediador.Send(solicitud, ct);
        return CreatedAtAction(nameof(Obtener), new { id }, new { id });
    }

    /// <summary>Reprograma la cita para otra franja.</summary>
    [HttpPut("{id:long}/franja")]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Reprogramar(ulong id, CambioFranja datos, CancellationToken ct)
    {
        await mediador.Send(new ReprogramarCitaCommand(id, datos.FranjaId), ct);
        return NoContent();
    }

    /// <summary>Registra una cancelación conservando la consulta histórica.</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Cancelar(ulong id, CancellationToken ct)
    {
        await mediador.Send(new CerrarCitaCommand(id), ct);
        return NoContent();
    }

    /// <summary>Registra que el propietario no presentó la motocicleta.</summary>
    [HttpPost("{id:long}/no-asistio")]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> RegistrarAusencia(ulong id, CancellationToken ct)
    {
        await mediador.Send(new CerrarCitaCommand(id, NoAsistio: true), ct);
        return NoContent();
    }
}

/// <summary>Franja de destino de una cita existente.</summary>
public sealed record CambioFranja(ulong FranjaId);
