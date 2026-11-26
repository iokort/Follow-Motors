namespace FollowMotors.Application.Operaciones;

using MediatR;

/// <summary>Usuario autenticado que realiza la operación; nunca se toma del cuerpo HTTP.</summary>
public sealed record ActorTaller(ulong UsuarioId, string Rol);
/// <summary>Tarea ponderada de un presupuesto; la suma de pesos debe ser 100.</summary>
public sealed record TareaPropuesta(string Descripcion, decimal Peso);
public sealed record CrearPresupuestoCommand(ulong AtencionId, ulong TipoServicioId, string Descripcion,
    decimal ManoObra, decimal Refacciones, DateTime? EntregaEstimada,
    List<TareaPropuesta> Tareas) : IRequest<ulong>;
public sealed record DecidirPresupuestoCommand(ulong PresupuestoId, bool Aceptar) : IRequest<bool>;
public sealed record ConsultarPresupuestoQuery(ulong PresupuestoId) : IRequest<PresupuestoVista?>;
public sealed record ContratarServicioCommand(ulong PresupuestoId) : IRequest<ulong>;
public sealed record AsignarMecanicoCommand(ulong OrdenId, ulong MecanicoId) : IRequest<bool>;
public sealed record ActualizarTareaCommand(ulong OrdenId, ulong TareaId, decimal Porcentaje, string? Evidencia) : IRequest<bool>;
public sealed record FinalizarOrdenCommand(ulong OrdenId) : IRequest<bool>;
public sealed record EntregarOrdenCommand(ulong OrdenId, string? Comprobante) : IRequest<bool>;
public sealed record ConsultarOrdenQuery(ulong OrdenId) : IRequest<OrdenVista?>;
public sealed record SolicitarRefaccionCommand(ulong PresupuestoId, string Descripcion, int Cantidad, decimal PrecioUnitario) : IRequest<ulong>;
public sealed record RecibirRefaccionCommand(ulong RefaccionId, int Cantidad, decimal CostoUnitario, string? Factura) : IRequest<bool>;
public sealed record ConsumirRefaccionCommand(ulong RefaccionId, int Cantidad) : IRequest<bool>;
public sealed record DevolverRefaccionCommand(ulong RefaccionId, int Cantidad) : IRequest<bool>;
public sealed record RegistrarAnticipoCommand(ulong OrdenId, decimal ManoObra, decimal Refacciones, string? Comprobante) : IRequest<ulong>;
public sealed record SolicitarCancelacionCommand(ulong OrdenId) : IRequest<ulong>;
public sealed record ResolverCancelacionCommand(ulong SolicitudId, bool Aprobar, string? MotivoRechazo) : IRequest<bool>;
public sealed record ConsultarCancelacionQuery(ulong SolicitudId) : IRequest<CancelacionVista?>;
public sealed record ProgramarMantenimientoCommand(ulong MotocicletaId, ulong? OrdenOrigenId,
    string Descripcion, DateTime? FechaEstimada, uint? KilometrajeObjetivo) : IRequest<ulong>;
public sealed record ConsultarMantenimientosQuery(ulong MotocicletaId) : IRequest<IReadOnlyList<MantenimientoVista>>;
public sealed record ConsultarNotificacionesQuery() : IRequest<IReadOnlyList<NotificacionVista>>;
public sealed record MarcarNotificacionLeidaCommand(ulong NotificacionId) : IRequest<bool>;
public sealed record ConsultarGarantiaQuery(ulong OrdenId) : IRequest<GarantiaVista?>;

/// <summary>Respuesta de consulta sin exponer información privada del personal.</summary>
public sealed record PresupuestoVista(ulong Id, ulong AtencionId, string DescripcionTrabajo, decimal ManoObra,
    decimal Refacciones, decimal Total, string Estado, DateTime? EntregaEstimada);
public sealed record OrdenVista(ulong Id, ulong PresupuestoId, string Estado, decimal Avance,
    DateTime? TerminadaEn, DateTime? EntregadaEn);
public sealed record CancelacionVista(ulong Id, ulong OrdenId, string Estado, decimal? Reembolso, string? MotivoRechazo);
public sealed record MantenimientoVista(ulong Id, ulong MotocicletaId, string Descripcion, DateTime? FechaEstimada,
    uint? KilometrajeObjetivo, string Estado);
public sealed record NotificacionVista(ulong Id, string Tipo, string Mensaje, DateTime CreadaEn, DateTime? LeidaEn);
public sealed record GarantiaVista(ulong Id, ulong OrdenId, bool Aplica, uint DuracionDias, string Condiciones,
    DateTime? IniciaEn, DateTime? VenceEn);

/// <summary>Contrato de persistencia transaccional: no define reglas HTTP ni conoce MediatR.</summary>
public interface IOperacionesTaller
{
    Task<ulong> CrearPresupuesto(CrearPresupuestoCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> DecidirPresupuesto(DecidirPresupuestoCommand comando, ActorTaller actor, CancellationToken ct);
    Task<PresupuestoVista?> ConsultarPresupuesto(ulong id, ActorTaller actor, CancellationToken ct);
    Task<ulong> Contratar(ulong presupuestoId, ActorTaller actor, CancellationToken ct);
    Task<bool> AsignarMecanico(AsignarMecanicoCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> AvanzarTarea(ActualizarTareaCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> Finalizar(ulong ordenId, ActorTaller actor, CancellationToken ct);
    Task<bool> Entregar(EntregarOrdenCommand comando, ActorTaller actor, CancellationToken ct);
    Task<OrdenVista?> ConsultarOrden(ulong id, ActorTaller actor, CancellationToken ct);
    Task<ulong> SolicitarRefaccion(SolicitarRefaccionCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> RecibirRefaccion(RecibirRefaccionCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> ConsumirRefaccion(ConsumirRefaccionCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> DevolverRefaccion(DevolverRefaccionCommand comando, ActorTaller actor, CancellationToken ct);
    Task<ulong> RegistrarAnticipo(RegistrarAnticipoCommand comando, ActorTaller actor, CancellationToken ct);
    Task<ulong> SolicitarCancelacion(ulong ordenId, ActorTaller actor, CancellationToken ct);
    Task<bool> ResolverCancelacion(ResolverCancelacionCommand comando, ActorTaller actor, CancellationToken ct);
    Task<CancelacionVista?> ConsultarCancelacion(ulong id, ActorTaller actor, CancellationToken ct);
    Task<ulong> ProgramarMantenimiento(ProgramarMantenimientoCommand comando, ActorTaller actor, CancellationToken ct);
    Task<IReadOnlyList<MantenimientoVista>> Mantenimientos(ulong motoId, ActorTaller actor, CancellationToken ct);
    Task<IReadOnlyList<NotificacionVista>> Notificaciones(ActorTaller actor, CancellationToken ct);
    Task<bool> MarcarNotificacion(ulong id, ActorTaller actor, CancellationToken ct);
    Task<GarantiaVista?> ConsultarGarantia(ulong ordenId, ActorTaller actor, CancellationToken ct);
}
