namespace FollowMotors.Application.Operaciones;

using FollowMotors.Application.Abstractions.Autenticacion;
using FollowMotors.Domain.Rules.Operaciones;
using MediatR;

/// <summary>Centraliza identidad y permisos de los casos de uso, sin repetirlos en controladores.</summary>
public abstract class CasoUsoTaller(IUsuarioActual usuario)
{
    protected ActorTaller Actor()
    {
        if (usuario.UsuarioId is null) throw new UnauthorizedAccessException("La sesión es obligatoria.");
        return new ActorTaller(usuario.UsuarioId.Value, usuario.Rol);
    }

    protected ActorTaller Empleado()
    {
        var actor = Actor();
        if (actor.Rol is not ("GERENTE" or "EMPLEADO"))
            throw new UnauthorizedAccessException("Se requiere personal autorizado.");
        return actor;
    }

    protected ActorTaller Gerente()
    {
        var actor = Actor();
        if (actor.Rol != "GERENTE")
            throw new UnauthorizedAccessException("Se requiere rol de gerencia.");
        return actor;
    }
}

/// <summary>Casos de uso de presupuestos y contratación; validan el dominio antes de persistir.</summary>
public sealed class PresupuestosTallerHandler(IOperacionesTaller repositorio, IUsuarioActual usuario) : CasoUsoTaller(usuario),
    IRequestHandler<CrearPresupuestoCommand, ulong>, IRequestHandler<DecidirPresupuestoCommand, bool>,
    IRequestHandler<ConsultarPresupuestoQuery, PresupuestoVista?>, IRequestHandler<ContratarServicioCommand, ulong>
{
    /// <summary>Valida importes y pesos antes de crear un presupuesto transaccional.</summary>
    public Task<ulong> Handle(CrearPresupuestoCommand c, CancellationToken ct)
    {
        if (c.Tareas is null) throw new ArgumentException("Las tareas son obligatorias.");
        ReglasOperacionesTaller.ValidarPresupuesto(c.Descripcion, c.ManoObra, c.Refacciones, c.Tareas.Select(t => t.Peso));
        if (c.Tareas.Any(t => string.IsNullOrWhiteSpace(t.Descripcion) || t.Descripcion.Length > 300))
            throw new ArgumentException("Las tareas deben tener descripciones válidas.");
        return repositorio.CrearPresupuesto(c, Empleado(), ct);
    }

    /// <summary>Autoriza la decisión, sin alterar posteriormente un presupuesto aceptado.</summary>
    public Task<bool> Handle(DecidirPresupuestoCommand c, CancellationToken ct) =>
        repositorio.DecidirPresupuesto(c, Empleado(), ct);

    /// <summary>Filtra la lectura por propiedad para impedir acceso a expedientes ajenos.</summary>
    public Task<PresupuestoVista?> Handle(ConsultarPresupuestoQuery c, CancellationToken ct) =>
        repositorio.ConsultarPresupuesto(c.PresupuestoId, Actor(), ct);

    /// <summary>Crea una orden solamente desde un presupuesto aceptado.</summary>
    public Task<ulong> Handle(ContratarServicioCommand c, CancellationToken ct) =>
        repositorio.Contratar(c.PresupuestoId, Empleado(), ct);
}

/// <summary>Orquesta el avance técnico y el cierre de órdenes sin acceder a tablas.</summary>
public sealed class EjecucionTallerHandler(IOperacionesTaller repositorio, IUsuarioActual usuario) : CasoUsoTaller(usuario),
    IRequestHandler<AsignarMecanicoCommand, bool>, IRequestHandler<ActualizarTareaCommand, bool>,
    IRequestHandler<FinalizarOrdenCommand, bool>, IRequestHandler<EntregarOrdenCommand, bool>,
    IRequestHandler<ConsultarOrdenQuery, OrdenVista?>
{
    /// <summary>Asigna un mecánico activo y disponible.</summary>
    public Task<bool> Handle(AsignarMecanicoCommand c, CancellationToken ct) => repositorio.AsignarMecanico(c, Gerente(), ct);
    /// <summary>Registra el progreso real de la tarea con evidencia opcional.</summary>
    public Task<bool> Handle(ActualizarTareaCommand c, CancellationToken ct)
    {
        ReglasOperacionesTaller.ValidarPorcentaje(c.Porcentaje);
        return repositorio.AvanzarTarea(c, Empleado(), ct);
    }
    /// <summary>Finaliza una orden únicamente cuando todas las tareas y piezas están resueltas.</summary>
    public Task<bool> Handle(FinalizarOrdenCommand c, CancellationToken ct) => repositorio.Finalizar(c.OrdenId, Empleado(), ct);
    /// <summary>Registra la entrega y activa la cobertura de garantía.</summary>
    public Task<bool> Handle(EntregarOrdenCommand c, CancellationToken ct) => repositorio.Entregar(c, Empleado(), ct);
    /// <summary>Devuelve el estado real de avance a usuarios autorizados.</summary>
    public Task<OrdenVista?> Handle(ConsultarOrdenQuery c, CancellationToken ct) => repositorio.ConsultarOrden(c.OrdenId, Actor(), ct);
}

/// <summary>Opera refacciones y pagos; centraliza las comprobaciones numéricas.</summary>
public sealed class RecursosTallerHandler(IOperacionesTaller repositorio, IUsuarioActual usuario) : CasoUsoTaller(usuario),
    IRequestHandler<SolicitarRefaccionCommand, ulong>, IRequestHandler<RecibirRefaccionCommand, bool>,
    IRequestHandler<ConsumirRefaccionCommand, bool>, IRequestHandler<DevolverRefaccionCommand, bool>,
    IRequestHandler<RegistrarAnticipoCommand, ulong>
{
    /// <summary>Solicita material para un presupuesto antes de su ejecución.</summary>
    public Task<ulong> Handle(SolicitarRefaccionCommand c, CancellationToken ct)
    {
        ReglasOperacionesTaller.ValidarCantidad(c.Cantidad);
        if (string.IsNullOrWhiteSpace(c.Descripcion) || c.Descripcion.Length > 300 || c.PrecioUnitario < 0)
            throw new ArgumentException("Refacción o precio inválidos.");
        return repositorio.SolicitarRefaccion(c, Empleado(), ct);
    }
    /// <summary>Recibe material sin exceder la cantidad solicitada.</summary>
    public Task<bool> Handle(RecibirRefaccionCommand c, CancellationToken ct)
    {
        ReglasOperacionesTaller.ValidarCantidad(c.Cantidad);
        if (c.CostoUnitario < 0) throw new ArgumentException("El costo debe ser no negativo.");
        return repositorio.RecibirRefaccion(c, Empleado(), ct);
    }
    /// <summary>Registra el consumo de unidades recibidas.</summary>
    public Task<bool> Handle(ConsumirRefaccionCommand c, CancellationToken ct)
    {
        ReglasOperacionesTaller.ValidarCantidad(c.Cantidad);
        return repositorio.ConsumirRefaccion(c, Empleado(), ct);
    }
    /// <summary>Devuelve material recibido que no haya sido consumido.</summary>
    public Task<bool> Handle(DevolverRefaccionCommand c, CancellationToken ct)
    {
        ReglasOperacionesTaller.ValidarCantidad(c.Cantidad);
        return repositorio.DevolverRefaccion(c, Empleado(), ct);
    }
    /// <summary>Registra el único anticipo permitido, sin exceder lo pactado.</summary>
    public Task<ulong> Handle(RegistrarAnticipoCommand c, CancellationToken ct)
    {
        ReglasOperacionesTaller.ValidarAnticipo(c.ManoObra, c.Refacciones);
        return repositorio.RegistrarAnticipo(c, Empleado(), ct);
    }
}

/// <summary>Coordina cancelación, postventa y notificaciones con control de acceso.</summary>
public sealed class PostventaTallerHandler(IOperacionesTaller repositorio, IUsuarioActual usuario) : CasoUsoTaller(usuario),
    IRequestHandler<SolicitarCancelacionCommand, ulong>, IRequestHandler<ResolverCancelacionCommand, bool>,
    IRequestHandler<ConsultarCancelacionQuery, CancelacionVista?>,
    IRequestHandler<ProgramarMantenimientoCommand, ulong>, IRequestHandler<ConsultarMantenimientosQuery, IReadOnlyList<MantenimientoVista>>,
    IRequestHandler<ConsultarNotificacionesQuery, IReadOnlyList<NotificacionVista>>,
    IRequestHandler<MarcarNotificacionLeidaCommand, bool>, IRequestHandler<ConsultarGarantiaQuery, GarantiaVista?>
{
    /// <summary>Solicita la cancelación de una orden propia o gestionada por personal.</summary>
    public Task<ulong> Handle(SolicitarCancelacionCommand c, CancellationToken ct) => repositorio.SolicitarCancelacion(c.OrdenId, Actor(), ct);
    /// <summary>Resuelve una solicitud con autorización de gerencia.</summary>
    public Task<bool> Handle(ResolverCancelacionCommand c, CancellationToken ct)
    {
        if (!c.Aprobar && string.IsNullOrWhiteSpace(c.MotivoRechazo))
            throw new ArgumentException("Indica el motivo de rechazo.");
        return repositorio.ResolverCancelacion(c, Gerente(), ct);
    }
    /// <summary>Consulta una solicitud respetando la propiedad de la orden.</summary>
    public Task<CancelacionVista?> Handle(ConsultarCancelacionQuery c, CancellationToken ct) =>
        repositorio.ConsultarCancelacion(c.SolicitudId, Actor(), ct);
    /// <summary>Agenda un mantenimiento para la motocicleta del expediente.</summary>
    public Task<ulong> Handle(ProgramarMantenimientoCommand c, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(c.Descripcion) || c.Descripcion.Length > 300)
            throw new ArgumentException("La descripción del mantenimiento es obligatoria.");
        return repositorio.ProgramarMantenimiento(c, Empleado(), ct);
    }
    /// <summary>Lista mantenimientos evitando filtraciones de motocicletas ajenas.</summary>
    public Task<IReadOnlyList<MantenimientoVista>> Handle(ConsultarMantenimientosQuery c, CancellationToken ct) =>
        repositorio.Mantenimientos(c.MotocicletaId, Actor(), ct);
    /// <summary>Obtiene notificaciones vinculadas a la cuenta autenticada.</summary>
    public Task<IReadOnlyList<NotificacionVista>> Handle(ConsultarNotificacionesQuery c, CancellationToken ct) =>
        repositorio.Notificaciones(Actor(), ct);
    /// <summary>Marca una notificación únicamente cuando pertenece al cliente.</summary>
    public Task<bool> Handle(MarcarNotificacionLeidaCommand c, CancellationToken ct) =>
        repositorio.MarcarNotificacion(c.NotificacionId, Actor(), ct);
    /// <summary>Consulta condiciones y vigencia de garantía de una orden propia.</summary>
    public Task<GarantiaVista?> Handle(ConsultarGarantiaQuery c, CancellationToken ct) =>
        repositorio.ConsultarGarantia(c.OrdenId, Actor(), ct);
}
