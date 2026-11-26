namespace FollowMotors.Application.Operaciones;

using FollowMotors.Application.Abstractions.Autenticacion;
using MediatR;

/// <summary>Centraliza identidad y permisos de los casos de uso, sin repetirlos en controladores.</summary>
public abstract class CasoUsoTaller(IUsuarioActual usuario)
{
    /// <summary>Obtiene la identidad firmada de la sesión.</summary>
    protected ActorTaller Actor()
    {
        if (usuario.UsuarioId is null) throw new UnauthorizedAccessException("La sesión es obligatoria.");
        return new ActorTaller(usuario.UsuarioId.Value, usuario.Rol);
    }

    /// <summary>Limita la operación a personal del taller.</summary>
    protected ActorTaller Empleado()
    {
        var actor = Actor();
        if (actor.Rol is not ("GERENTE" or "EMPLEADO"))
            throw new UnauthorizedAccessException("Se requiere personal autorizado.");
        return actor;
    }

    /// <summary>Limita la operación a gerencia.</summary>
    protected ActorTaller Gerente()
    {
        var actor = Actor();
        if (actor.Rol != "GERENTE")
            throw new UnauthorizedAccessException("Se requiere rol de gerencia.");
        return actor;
    }
}

/// <summary>Orquesta el presupuesto, el acuerdo presencial y el anticipo de reparación.</summary>
public sealed class PresupuestosTallerHandler(IPresupuestosTaller repositorio, IUsuarioActual usuario) : CasoUsoTaller(usuario),
    IRequestHandler<CrearPresupuestoCommand, ulong>, IRequestHandler<DecidirPresupuestoCommand, bool>,
    IRequestHandler<ConsultarPresupuestoQuery, PresupuestoVista?>, IRequestHandler<ContratarServicioCommand, ulong>,
    IRequestHandler<RegistrarAnticipoCommand, ulong>, IRequestHandler<RegistrarTipoServicioCommand, ulong>,
    IRequestHandler<TiposServicioQuery, IReadOnlyList<TipoServicioVista>>
{
    /// <summary>Registra el presupuesto con las tareas validadas del servicio.</summary>
    public Task<ulong> Handle(CrearPresupuestoCommand solicitud, CancellationToken ct) =>
        repositorio.CrearPresupuesto(solicitud, Empleado(), ct);

    /// <summary>Registra la decisión que el personal verificó presencialmente.</summary>
    public Task<bool> Handle(DecidirPresupuestoCommand solicitud, CancellationToken ct) =>
        repositorio.DecidirPresupuesto(solicitud, Empleado(), ct);

    /// <summary>Obtiene una cotización visible para la sesión.</summary>
    public Task<PresupuestoVista?> Handle(ConsultarPresupuestoQuery consulta, CancellationToken ct) =>
        repositorio.ConsultarPresupuesto(consulta.PresupuestoId, Actor(), ct);

    /// <summary>Contrata el servicio y registra simultáneamente su anticipo.</summary>
    public Task<ulong> Handle(ContratarServicioCommand solicitud, CancellationToken ct) =>
        repositorio.Contratar(solicitud, Empleado(), ct);

    /// <summary>Completa el registro de un anticipo para una orden previa que carece de pago.</summary>
    public Task<ulong> Handle(RegistrarAnticipoCommand solicitud, CancellationToken ct) =>
        repositorio.RegistrarAnticipo(solicitud, Empleado(), ct);

    /// <summary>Permite a gerencia registrar las condiciones de un tipo de servicio.</summary>
    public Task<ulong> Handle(RegistrarTipoServicioCommand solicitud, CancellationToken ct) =>
        repositorio.RegistrarTipoServicio(solicitud, Gerente(), ct);

    /// <summary>Recupera los tipos de servicio disponibles para que el personal cotice.</summary>
    public Task<IReadOnlyList<TipoServicioVista>> Handle(TiposServicioQuery consulta, CancellationToken ct)
    {
        Empleado();
        return repositorio.TiposServicio(ct);
    }
}

/// <summary>Coordina la ejecución técnica y el cierre de órdenes.</summary>
public sealed class EjecucionTallerHandler(IEjecucionTaller repositorio, IUsuarioActual usuario) : CasoUsoTaller(usuario),
    IRequestHandler<AsignarMecanicoCommand, bool>, IRequestHandler<ActualizarTareaCommand, bool>,
    IRequestHandler<FinalizarOrdenCommand, bool>, IRequestHandler<EntregarOrdenCommand, bool>,
    IRequestHandler<EstimarEntregaCommand, bool>, IRequestHandler<ConsultarOrdenQuery, OrdenVista?>
{
    /// <summary>Reasigna al responsable activo por autorización gerencial.</summary>
    public Task<bool> Handle(AsignarMecanicoCommand solicitud, CancellationToken ct) =>
        repositorio.AsignarMecanico(solicitud, Gerente(), ct);

    /// <summary>Registra la ejecución verificable del trabajo del mecánico asignado.</summary>
    public Task<bool> Handle(ActualizarTareaCommand solicitud, CancellationToken ct) =>
        repositorio.AvanzarTarea(solicitud, Empleado(), ct);

    /// <summary>Actualiza la estimación de entrega sin tocar los pesos del plan.</summary>
    public Task<bool> Handle(EstimarEntregaCommand solicitud, CancellationToken ct) =>
        repositorio.EstimarEntrega(solicitud, Empleado(), ct);

    /// <summary>Confirma la finalización técnica según el trabajo realmente realizado.</summary>
    public Task<bool> Handle(FinalizarOrdenCommand solicitud, CancellationToken ct) =>
        repositorio.Finalizar(solicitud.OrdenId, Empleado(), ct);

    /// <summary>Confirma la entrega de la unidad por parte del personal.</summary>
    public Task<bool> Handle(EntregarOrdenCommand solicitud, CancellationToken ct) =>
        repositorio.Entregar(solicitud, Empleado(), ct);

    /// <summary>Consulta detalles y progreso de una orden accesible a la sesión.</summary>
    public Task<OrdenVista?> Handle(ConsultarOrdenQuery consulta, CancellationToken ct) =>
        repositorio.ConsultarOrden(consulta.OrdenId, Actor(), ct);
}

/// <summary>Coordina piezas cotizadas y movimientos del material del proveedor.</summary>
public sealed class RefaccionesTallerHandler(IRefaccionesTaller repositorio, IUsuarioActual usuario) : CasoUsoTaller(usuario),
    IRequestHandler<SolicitarRefaccionCommand, ulong>, IRequestHandler<PedirRefaccionCommand, bool>,
    IRequestHandler<RecibirRefaccionCommand, bool>, IRequestHandler<ConsumirRefaccionCommand, bool>,
    IRequestHandler<DevolverRefaccionCommand, bool>
{
    /// <summary>Agrega una pieza a la cotización antes de confirmar el presupuesto.</summary>
    public Task<ulong> Handle(SolicitarRefaccionCommand solicitud, CancellationToken ct) =>
        repositorio.SolicitarRefaccion(solicitud, Empleado(), ct);

    /// <summary>Registra el pedido que el personal hizo al proveedor.</summary>
    public Task<bool> Handle(PedirRefaccionCommand solicitud, CancellationToken ct) =>
        repositorio.PedirRefaccion(solicitud.RefaccionId, Empleado(), ct);

    /// <summary>Registra material recibido contra la pieza pedida.</summary>
    public Task<bool> Handle(RecibirRefaccionCommand solicitud, CancellationToken ct) =>
        repositorio.RecibirRefaccion(solicitud, Empleado(), ct);

    /// <summary>Registra el uso de unidades recibidas en la reparación.</summary>
    public Task<bool> Handle(ConsumirRefaccionCommand solicitud, CancellationToken ct) =>
        repositorio.ConsumirRefaccion(solicitud, Empleado(), ct);

    /// <summary>Registra el retorno real de unidades selladas no consumidas.</summary>
    public Task<bool> Handle(DevolverRefaccionCommand solicitud, CancellationToken ct) =>
        repositorio.DevolverRefaccion(solicitud, Empleado(), ct);
}

/// <summary>Aplica permisos en cancelaciones, mantenimiento y consultas de seguimiento.</summary>
public sealed class PostventaTallerHandler(IPostventaTaller repositorio, IUsuarioActual usuario) : CasoUsoTaller(usuario),
    IRequestHandler<SolicitarCancelacionCommand, ulong>, IRequestHandler<ResolverCancelacionCommand, bool>,
    IRequestHandler<RegistrarReembolsoCommand, bool>, IRequestHandler<ConsultarCancelacionQuery, CancelacionVista?>,
    IRequestHandler<ProgramarMantenimientoCommand, ulong>, IRequestHandler<ConsultarMantenimientosQuery, IReadOnlyList<MantenimientoVista>>,
    IRequestHandler<ConsultarNotificacionesQuery, IReadOnlyList<NotificacionVista>>,
    IRequestHandler<MarcarNotificacionLeidaCommand, bool>, IRequestHandler<ConsultarGarantiaQuery, GarantiaVista?>
{
    /// <summary>Registra la petición del titular o de personal que le asiste.</summary>
    public Task<ulong> Handle(SolicitarCancelacionCommand solicitud, CancellationToken ct) =>
        repositorio.SolicitarCancelacion(solicitud.OrdenId, Actor(), ct);

    /// <summary>Resuelve la solicitud mediante autorización gerencial.</summary>
    public Task<bool> Handle(ResolverCancelacionCommand solicitud, CancellationToken ct) =>
        repositorio.ResolverCancelacion(solicitud, Gerente(), ct);

    /// <summary>Confirma la entrega manual del importe pendiente.</summary>
    public Task<bool> Handle(RegistrarReembolsoCommand solicitud, CancellationToken ct) =>
        repositorio.RegistrarReembolso(solicitud, Gerente(), ct);

    /// <summary>Consulta el desglose de una solicitud visible para la sesión.</summary>
    public Task<CancelacionVista?> Handle(ConsultarCancelacionQuery consulta, CancellationToken ct) =>
        repositorio.ConsultarCancelacion(consulta.SolicitudId, Actor(), ct);

    /// <summary>Programa un mantenimiento mediante personal autorizado.</summary>
    public Task<ulong> Handle(ProgramarMantenimientoCommand solicitud, CancellationToken ct) =>
        repositorio.ProgramarMantenimiento(solicitud, Empleado(), ct);

    /// <summary>Lista el mantenimiento para una motocicleta accesible.</summary>
    public Task<IReadOnlyList<MantenimientoVista>> Handle(ConsultarMantenimientosQuery consulta, CancellationToken ct) =>
        repositorio.Mantenimientos(consulta.MotocicletaId, Actor(), ct);

    /// <summary>Obtiene notificaciones vinculadas a la cuenta autenticada.</summary>
    public Task<IReadOnlyList<NotificacionVista>> Handle(ConsultarNotificacionesQuery consulta, CancellationToken ct) =>
        repositorio.Notificaciones(Actor(), ct);

    /// <summary>Actualiza el estado de lectura de una notificación propia.</summary>
    public Task<bool> Handle(MarcarNotificacionLeidaCommand solicitud, CancellationToken ct) =>
        repositorio.MarcarNotificacion(solicitud.NotificacionId, Actor(), ct);

    /// <summary>Consulta condiciones y estado de una garantía autorizada.</summary>
    public Task<GarantiaVista?> Handle(ConsultarGarantiaQuery consulta, CancellationToken ct) =>
        repositorio.ConsultarGarantia(consulta.OrdenId, Actor(), ct);
}
