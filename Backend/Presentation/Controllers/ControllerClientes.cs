namespace FollowMotors.Presentation.Controllers.Clientes;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FollowMotors.Application.Commands.Clientes;
using FollowMotors.Application.Queries.Clientes;

/// <summary>Endpoints del alta presencial, baja lógica y expediente privado.</summary>
[ApiController]
[Route("api/clientes")]
[Authorize]
public sealed class ClientesController(ISender mediador) : ControllerBase
{
    /// <summary>Solicita el registro presencial de un cliente sin lógica en HTTP.</summary>
    [HttpPost]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Registrar([FromBody] CrearCuentaClienteCommand solicitud,
        CancellationToken ct)
    {
        ulong id = await mediador.Send(solicitud, ct);
        return Created($"/api/clientes/{id}", new { id });
    }

    /// <summary>Desactiva la cuenta solo cuando se identifica como gerente.</summary>
    [HttpDelete("{clienteId:long}")]
    [Authorize(Roles = "GERENTE")]
    public async Task<IActionResult> Desactivar(ulong clienteId, CancellationToken ct) =>
        await mediador.Send(new DesactivarCuentaClienteCommand(clienteId), ct)
            ? NoContent() : NotFound();

    /// <summary>Devuelve al propietario su propio expediente, sin parámetro de identidad.</summary>
    [HttpGet("mi-expediente")]
    [Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> MiExpediente(CancellationToken ct) =>
        Ok(await mediador.Send(new ConsultarExpedientePropioQuery(), ct));

    /// <summary>Busca expedientes para personal autorizado.</summary>
    [HttpGet]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Buscar(string? buscar, int pagina = 1, CancellationToken ct = default) =>
        Ok(await mediador.Send(new BuscarClientesQuery(buscar, pagina), ct));

    /// <summary>Devuelve la ficha administrativa del expediente.</summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Obtener(ulong id, CancellationToken ct) =>
        Ok(await mediador.Send(new ObtenerClienteQuery(id), ct));

    /// <summary>Actualiza los datos de contacto del expediente.</summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "GERENTE,EMPLEADO")]
    public async Task<IActionResult> Actualizar(ulong id, DatosCliente datos, CancellationToken ct)
    {
        await mediador.Send(new ActualizarClienteCommand(id, datos), ct);
        return NoContent();
    }
}
