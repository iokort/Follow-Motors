namespace FollowMotors.Presentation.Controllers.Atenciones;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FollowMotors.Application.Commands.Atenciones;
using FollowMotors.Application.Queries.Atenciones;

/// <summary>HTTP de ingresos, diagnóstico, cobro y consulta autorizada.</summary>
[ApiController]
[Route("api/atenciones")]
[Authorize]
public sealed class AtencionesController(ISender mediador) : ControllerBase
{
    /// <summary>Recibe una moto y registra la atención con el mecánico asignado.</summary>
    [HttpPost]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Recibir([FromBody] RecepcionarMotocicletaCommand solicitud,
        CancellationToken ct)
    {
        ulong id = await mediador.Send(solicitud, ct);
        return Created($"/api/atenciones/{id}", new { id });
    }

    /// <summary>Delega al caso de uso la anotación de hallazgos técnicos.</summary>
    [HttpPost("{atencionId:long}/diagnostico")]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Diagnosticar(ulong atencionId,
        [FromBody] DatosDiagnostico entrada, CancellationToken ct) =>
        await mediador.Send(new RegistrarDiagnosticoCommand(atencionId, entrada.Hallazgos), ct)
            ? NoContent() : Conflict();

    /// <summary>Delega el cobro único según la tarifa registrada en la base.</summary>
    [HttpPost("{atencionId:long}/cobro")]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Cobrar(ulong atencionId,
        [FromBody] DatosCobro entrada, CancellationToken ct) =>
        Ok(new { id = await mediador.Send(
            new CobrarRevisionCommand(atencionId, entrada.ReferenciaComprobante), ct) });

    /// <summary>Consulta una atención tras la autorización por propietario.</summary>
    [HttpGet("{atencionId:long}")]
    public async Task<IActionResult> Consultar(ulong atencionId, CancellationToken ct)
    {
        var atencion = await mediador.Send(new ConsultarAtencionQuery(atencionId), ct);
        return atencion is null ? NotFound() : Ok(atencion);
    }
    /// <summary>Cierra la revisión cobrada que no se convirtió en reparación.</summary>
    [HttpPost("{atencionId:long}/cerrar-sin-reparacion")]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> CerrarRevision(ulong atencionId, CancellationToken ct)
    {
        await mediador.Send(new CerrarRevisionCommand(atencionId), ct);
        return NoContent();
    }

}

/// <summary>Único dato de entrada necesario para registrar el diagnóstico.</summary>
public sealed record DatosDiagnostico(string Hallazgos);

/// <summary>Referencia opcional de comprobante de pago.</summary>
public sealed record DatosCobro(string? ReferenciaComprobante);
