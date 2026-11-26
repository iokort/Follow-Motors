namespace FollowMotors.Presentation.Controllers.Gerencia;

using MediatR;
using FollowMotors.Application.Operaciones;
using FollowMotors.Application.Commands.Atenciones;
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

    /// <summary>Establece una tarifa fija para las siguientes revisiones.</summary>
    [HttpPut("tarifa-revision")]
    public async Task<IActionResult> TarifaRevision(ConfigurarTarifaRevisionCommand solicitud, CancellationToken ct) =>
        Ok(new { id = await mediador.Send(solicitud, ct) });

    /// <summary>Consulta los indicadores gerenciales del periodo seleccionado.</summary>
    [HttpGet("reporte-operativo")]
    public async Task<IActionResult> ReporteOperativo(DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) =>
        Ok(await mediador.Send(new ReporteOperativoQuery(desde, hasta), ct));

    /// <summary>Genera de inmediato los recordatorios elegibles del canal interno.</summary>
    [HttpPost("recordatorios/generar")]
    public async Task<IActionResult> GenerarRecordatorios(CancellationToken ct) =>
        Ok(new { generados = await mediador.Send(new GenerarRecordatoriosCommand(), ct) });
}
