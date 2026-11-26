namespace FollowMotors.Presentation.Controllers.Operaciones;

using FollowMotors.Application.Operaciones;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>API complementaria de posventa para el entorno local, sobre CQRS/MediatR.</summary>
[ApiController]
[Authorize]
[Route("api/operaciones")]
public sealed class OperacionesTallerController(ISender mediator) : ControllerBase
{
    /// <summary>Registra el presupuesto con su plan técnico ponderado.</summary>
    [HttpPost("presupuestos")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> CrearPresupuesto(CrearPresupuestoCommand request,CancellationToken ct)
    {
        var id=await mediator.Send(request,ct);
        return Created($"/api/operaciones/presupuestos/{id}",new{id});
    }
    /// <summary>Acepta o rechaza un presupuesto pendiente.</summary>
    [HttpPost("presupuestos/decision")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Decidir(DecidirPresupuestoCommand request,CancellationToken ct) =>
        await Resultado(mediator.Send(request,ct));
    /// <summary>Consulta un presupuesto visible para la sesión.</summary>
    [HttpGet("presupuestos/{id:long}")]
    public async Task<IActionResult> Presupuesto(ulong id,CancellationToken ct) =>
        (await mediator.Send(new ConsultarPresupuestoQuery(id),ct)) is { } dato?Ok(dato):NotFound();
    /// <summary>Abre la orden de servicio desde un presupuesto aceptado.</summary>
    [HttpPost("ordenes")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Contratar(ContratarServicioCommand request,CancellationToken ct) =>
        Ok(new {id=await mediator.Send(request,ct)});
    /// <summary>Reasigna al técnico responsable de la orden.</summary>
    [HttpPost("ordenes/mecanico")][Authorize(Roles="GERENTE")]
    public async Task<IActionResult> Mecanico(AsignarMecanicoCommand request,CancellationToken ct) =>
        await Resultado(mediator.Send(request,ct));
    /// <summary>Registra porcentaje de avance de una tarea real.</summary>
    [HttpPost("ordenes/tareas/avance")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Avance(ActualizarTareaCommand request,CancellationToken ct) =>
        await Resultado(mediator.Send(request,ct));
    /// <summary>Finaliza la reparación únicamente al completar todas las tareas.</summary>
    [HttpPost("ordenes/{id:long}/finalizar")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Finalizar(ulong id,CancellationToken ct) =>
        await Resultado(mediator.Send(new FinalizarOrdenCommand(id),ct));
    /// <summary>Confirma la entrega y activa la garantía.</summary>
    [HttpPost("ordenes/entrega")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Entregar(EntregarOrdenCommand request,CancellationToken ct) =>
        await Resultado(mediator.Send(request,ct));
    /// <summary>Devuelve estado y progreso real de la orden.</summary>
    [HttpGet("ordenes/{id:long}")]
    public async Task<IActionResult> Orden(ulong id,CancellationToken ct) =>
        (await mediator.Send(new ConsultarOrdenQuery(id),ct)) is { } dato?Ok(dato):NotFound();
    /// <summary>Solicita una nueva refacción antes de congelar el presupuesto.</summary>
    [HttpPost("refacciones")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Refaccion(SolicitarRefaccionCommand request,CancellationToken ct) =>
        Ok(new {id=await mediator.Send(request,ct)});
    /// <summary>Registra unidades recibidas contra un pedido existente.</summary>
    [HttpPost("refacciones/recepcion")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Recepcion(RecibirRefaccionCommand request,CancellationToken ct) =>
        await Resultado(mediator.Send(request,ct));
    /// <summary>Descuenta el material utilizado de la disponibilidad real.</summary>
    [HttpPost("refacciones/consumo")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Consumo(ConsumirRefaccionCommand request,CancellationToken ct) =>
        await Resultado(mediator.Send(request,ct));
    /// <summary>Devuelve existencias no utilizadas.</summary>
    [HttpPost("refacciones/devolucion")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Devolucion(DevolverRefaccionCommand request,CancellationToken ct) =>
        await Resultado(mediator.Send(request,ct));
    /// <summary>Registra un anticipo limitado por el presupuesto autorizado.</summary>
    [HttpPost("anticipos")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Anticipo(RegistrarAnticipoCommand request,CancellationToken ct) =>
        Ok(new {id=await mediator.Send(request,ct)});
    /// <summary>Solicita cancelar una orden activa.</summary>
    [HttpPost("cancelaciones")]
    public async Task<IActionResult> Cancelar(SolicitarCancelacionCommand request,CancellationToken ct) =>
        Ok(new {id=await mediator.Send(request,ct)});
    /// <summary>Resuelve la solicitud y calcula el reintegro debido.</summary>
    [HttpPost("cancelaciones/resolver")][Authorize(Roles="GERENTE")]
    public async Task<IActionResult> Resolver(ResolverCancelacionCommand request,CancellationToken ct) =>
        await Resultado(mediator.Send(request,ct));
    /// <summary>Consulta el resultado de una cancelación.</summary>
    [HttpGet("cancelaciones/{id:long}")]
    public async Task<IActionResult> Cancelacion(ulong id,CancellationToken ct) =>
        (await mediator.Send(new ConsultarCancelacionQuery(id),ct)) is { } dato?Ok(dato):NotFound();
    /// <summary>Programa seguimiento preventivo de una motocicleta.</summary>
    [HttpPost("mantenimientos")][Authorize(Roles="GERENTE,EMPLEADO")]
    public async Task<IActionResult> Programar(ProgramarMantenimientoCommand request,CancellationToken ct) =>
        Ok(new {id=await mediator.Send(request,ct)});
    /// <summary>Consulta mantenimientos de una motocicleta del expediente.</summary>
    [HttpGet("mantenimientos/motocicleta/{id:long}")]
    public async Task<IActionResult> Mantenimientos(ulong id,CancellationToken ct) =>
        Ok(await mediator.Send(new ConsultarMantenimientosQuery(id),ct));
    /// <summary>Consulta las notificaciones locales de la sesión.</summary>
    [HttpGet("notificaciones")]
    public async Task<IActionResult> Notificaciones(CancellationToken ct) =>
        Ok(await mediator.Send(new ConsultarNotificacionesQuery(),ct));
    /// <summary>Marca una notificación como leída.</summary>
    [HttpPost("notificaciones/{id:long}/leer")]
    public async Task<IActionResult> Leer(ulong id,CancellationToken ct) =>
        await Resultado(mediator.Send(new MarcarNotificacionLeidaCommand(id),ct));
    /// <summary>Consulta la garantía registrada para la orden.</summary>
    [HttpGet("garantias/orden/{id:long}")]
    public async Task<IActionResult> Garantia(ulong id,CancellationToken ct) =>
        (await mediator.Send(new ConsultarGarantiaQuery(id),ct)) is { } dato?Ok(dato):NotFound();

    /// <summary>Convierte una transición condicional en HTTP 204 o 409 sin repetir el patrón.</summary>
    private async Task<IActionResult> Resultado(Task<bool> resultado) =>
        await resultado ? NoContent() : Conflict(new {mensaje="Estado o requisitos incompatibles."});
}
