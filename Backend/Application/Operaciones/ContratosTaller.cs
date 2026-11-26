namespace FollowMotors.Application.Operaciones;

using MediatR;

/// <summary>Usuario autenticado que realiza la operación; nunca se toma del cuerpo HTTP.</summary>
public sealed record ActorTaller(ulong UsuarioId, string Rol);
/// <summary>Tarea ponderada de un presupuesto; la suma de pesos debe ser 100.</summary>
public sealed record TareaPropuesta(string Descripcion, decimal Peso);
public sealed record CrearPresupuestoCommand(ulong AtencionId, ulong TipoServicioId, string Descripcion,
    decimal ManoObra, decimal Refacciones, DateTimeOffset? EntregaEstimada,
    List<TareaPropuesta> Tareas) : IRequest<ulong>;
public sealed record DecidirPresupuestoCommand(ulong PresupuestoId, bool Aceptar) : IRequest<bool>;
public sealed record ConsultarPresupuestoQuery(ulong PresupuestoId) : IRequest<PresupuestoVista?>;
/// <summary>Registra el acuerdo y el pago completo, con la referencia entregada presencialmente.</summary>
public sealed record ContratarServicioCommand(ulong PresupuestoId, decimal ManoObra, decimal Refacciones,
    string Comprobante) : IRequest<ulong>;
/// <summary>Registra en el catálogo el servicio y su cobertura de taller.</summary>
public sealed record RegistrarTipoServicioCommand(string Nombre, string? Descripcion,
    ushort GarantiaDias, string? Cobertura) : IRequest<ulong>;
/// <summary>Lista los tipos de servicio que se pueden cotizar.</summary>
public sealed record TiposServicioQuery : IRequest<IReadOnlyList<TipoServicioVista>>;
/// <summary>Tipo de reparación y condiciones disponibles para cotizar.</summary>
public sealed record TipoServicioVista(ulong Id, string Nombre, string? Descripcion,
    ushort GarantiaDias, string? Cobertura);
/// <summary>Registra una fecha de entrega prevista sin modificar el progreso.</summary>
public sealed record EstimarEntregaCommand(ulong OrdenId, DateTimeOffset EntregaEstimada) : IRequest<bool>;
public sealed record AsignarMecanicoCommand(ulong OrdenId, ulong MecanicoId) : IRequest<bool>;
public sealed record ActualizarTareaCommand(ulong OrdenId, ulong TareaId, decimal Porcentaje, string? Evidencia) : IRequest<bool>;
public sealed record FinalizarOrdenCommand(ulong OrdenId) : IRequest<bool>;
public sealed record EntregarOrdenCommand(ulong OrdenId, string? Comprobante) : IRequest<bool>;
public sealed record ConsultarOrdenQuery(ulong OrdenId) : IRequest<OrdenVista?>;
public sealed record SolicitarRefaccionCommand(ulong PresupuestoId, string Descripcion, int Cantidad, decimal PrecioUnitario) : IRequest<ulong>;
/// <summary>Registra que el pedido de una pieza ya cotizada se realizó al proveedor.</summary>
public sealed record PedirRefaccionCommand(ulong RefaccionId) : IRequest<bool>;
public sealed record RecibirRefaccionCommand(ulong RefaccionId, int Cantidad, decimal CostoUnitario, string? Factura) : IRequest<bool>;
public sealed record ConsumirRefaccionCommand(ulong RefaccionId, int Cantidad) : IRequest<bool>;
public sealed record DevolverRefaccionCommand(ulong RefaccionId, int Cantidad, bool Sellada) : IRequest<bool>;
public sealed record RegistrarAnticipoCommand(ulong OrdenId, decimal ManoObra, decimal Refacciones, string? Comprobante) : IRequest<ulong>;
public sealed record SolicitarCancelacionCommand(ulong OrdenId) : IRequest<ulong>;
public sealed record ResolverCancelacionCommand(ulong SolicitudId, bool Aprobar, string? MotivoRechazo) : IRequest<bool>;
/// <summary>Asienta el reintegro manual efectivamente entregado al cliente.</summary>
public sealed record RegistrarReembolsoCommand(ulong SolicitudId, decimal Importe, string Referencia) : IRequest<bool>;
public sealed record ConsultarCancelacionQuery(ulong SolicitudId) : IRequest<CancelacionVista?>;
public sealed record ProgramarMantenimientoCommand(ulong MotocicletaId, ulong? OrdenOrigenId,
    string Descripcion, DateTime? FechaEstimada, uint? KilometrajeObjetivo) : IRequest<ulong>;
public sealed record ConsultarMantenimientosQuery(ulong MotocicletaId) : IRequest<IReadOnlyList<MantenimientoVista>>;
public sealed record ConsultarNotificacionesQuery() : IRequest<IReadOnlyList<NotificacionVista>>;
public sealed record MarcarNotificacionLeidaCommand(ulong NotificacionId) : IRequest<bool>;
public sealed record ConsultarGarantiaQuery(ulong OrdenId) : IRequest<GarantiaVista?>;

/// <summary>Respuesta de consulta sin exponer información privada del personal.</summary>
public sealed record PresupuestoVista(ulong Id, ulong AtencionId, string DescripcionTrabajo, decimal ManoObra,
    decimal Refacciones, decimal Total, string Estado, DateTime? EntregaEstimada)
{
    public IReadOnlyList<TareaVista> Tareas { get; init; } = [];
    public IReadOnlyList<RefaccionVista> Piezas { get; init; } = [];
}
public sealed record OrdenVista(ulong Id, ulong PresupuestoId, string Estado, decimal Avance,
    DateTime? TerminadaEn, DateTime? EntregadaEn, DateTime? EntregaEstimada, ulong MecanicoId)
{
    public IReadOnlyList<TareaVista> Tareas { get; init; } = [];
    public IReadOnlyList<RefaccionVista> Piezas { get; init; } = [];
}
/// <summary>Progreso comunicable de una tarea aprobada del servicio.</summary>
public sealed record TareaVista(ulong Id, string Descripcion, decimal Peso, decimal Porcentaje,
    string Estado, DateTime? ActualizadaEn);
/// <summary>Cantidades y precio acordado de una pieza, sin costos internos del proveedor.</summary>
public sealed record RefaccionVista(ulong Id, string Descripcion, ushort Solicitadas, ushort Recibidas,
    ushort Utilizadas, ushort Devueltas, decimal PrecioUnitario, DateTime? SolicitadaEn);
/// <summary>Resolución y liquidación, con la fecha de devolución efectiva cuando existe.</summary>
public sealed record CancelacionVista(ulong Id, ulong OrdenId, string Estado, decimal? Reembolso,
    string? MotivoRechazo, decimal? Avance, decimal? ManoObraRealizada, decimal? PiezasUsadas,
    DateTime? ReembolsadaEn)
{
    public IReadOnlyList<RefaccionVista> PiezasPorDevolver { get; init; } = [];
}
public sealed record MantenimientoVista(ulong Id, ulong MotocicletaId, string Descripcion, DateTime? FechaEstimada,
    uint? KilometrajeObjetivo, string Estado);
public sealed record NotificacionVista(ulong Id, string Tipo, string Mensaje, DateTime CreadaEn, DateTime? LeidaEn);
public sealed record GarantiaVista(ulong Id, ulong OrdenId, bool Aplica, ushort DuracionDias, string Condiciones,
    DateTime? IniciaEn, DateTime? VenceEn, string? ReferenciaComprobante, string EstadoOrden)
{
    public string Estado { get; init; } = "";
}

/// <summary>Contrato de persistencia transaccional: no define reglas HTTP ni conoce MediatR.</summary>
public interface IPresupuestosTaller
{
    Task<ulong> CrearPresupuesto(CrearPresupuestoCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> DecidirPresupuesto(DecidirPresupuestoCommand comando, ActorTaller actor, CancellationToken ct);
    Task<PresupuestoVista?> ConsultarPresupuesto(ulong id, ActorTaller actor, CancellationToken ct);
    Task<ulong> Contratar(ContratarServicioCommand comando, ActorTaller actor, CancellationToken ct);
    Task<ulong> RegistrarTipoServicio(RegistrarTipoServicioCommand comando, ActorTaller actor, CancellationToken ct);
    Task<IReadOnlyList<TipoServicioVista>> TiposServicio(CancellationToken ct);
    Task<ulong> RegistrarAnticipo(RegistrarAnticipoCommand comando, ActorTaller actor, CancellationToken ct);
}

/// <summary>Persiste asignación, progreso, terminación y entrega de reparaciones.</summary>
public interface IEjecucionTaller
{
    Task<bool> AsignarMecanico(AsignarMecanicoCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> AvanzarTarea(ActualizarTareaCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> EstimarEntrega(EstimarEntregaCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> Finalizar(ulong ordenId, ActorTaller actor, CancellationToken ct);
    Task<bool> Entregar(EntregarOrdenCommand comando, ActorTaller actor, CancellationToken ct);
    Task<OrdenVista?> ConsultarOrden(ulong id, ActorTaller actor, CancellationToken ct);
}

/// <summary>Persiste las piezas de la cotización y los movimientos con el proveedor.</summary>
public interface IRefaccionesTaller
{
    Task<ulong> SolicitarRefaccion(SolicitarRefaccionCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> PedirRefaccion(ulong id, ActorTaller actor, CancellationToken ct);
    Task<bool> RecibirRefaccion(RecibirRefaccionCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> ConsumirRefaccion(ConsumirRefaccionCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> DevolverRefaccion(DevolverRefaccionCommand comando, ActorTaller actor, CancellationToken ct);
}

/// <summary>Persiste cancelación, reintegro, garantías y consultas de posventa.</summary>
public interface IPostventaTaller
{
    Task<ulong> SolicitarCancelacion(ulong ordenId, ActorTaller actor, CancellationToken ct);
    Task<bool> ResolverCancelacion(ResolverCancelacionCommand comando, ActorTaller actor, CancellationToken ct);
    Task<bool> RegistrarReembolso(RegistrarReembolsoCommand comando, ActorTaller actor, CancellationToken ct);
    Task<CancelacionVista?> ConsultarCancelacion(ulong id, ActorTaller actor, CancellationToken ct);
    Task<ulong> ProgramarMantenimiento(ProgramarMantenimientoCommand comando, ActorTaller actor, CancellationToken ct);
    Task<IReadOnlyList<MantenimientoVista>> Mantenimientos(ulong motoId, ActorTaller actor, CancellationToken ct);
    Task<IReadOnlyList<NotificacionVista>> Notificaciones(ActorTaller actor, CancellationToken ct);
    Task<bool> MarcarNotificacion(ulong id, ActorTaller actor, CancellationToken ct);
    Task<GarantiaVista?> ConsultarGarantia(ulong ordenId, ActorTaller actor, CancellationToken ct);
}
