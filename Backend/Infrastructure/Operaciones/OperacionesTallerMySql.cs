namespace FollowMotors.Infrastructure.Operaciones;

using System.Text.Json;
using Dapper;
using FollowMotors.Application.Operaciones;
using FollowMotors.Domain.Rules.Servicios;
using FollowMotors.Infrastructure.Database;
using MySqlConnector;

/// <summary>Persistencia de la reparación desde cotización hasta entrega y postventa.</summary>
public sealed partial class OperacionesTallerMySql(ConexionMySql baseDatos, TimeProvider reloj) : IOperacionesTaller
{
    private const string PropiedadPresupuesto = """
        (@Rol IN ('GERENTE','EMPLEADO') OR EXISTS (SELECT 1 FROM atenciones a JOIN clientes c ON c.id=a.cliente_id
            WHERE a.id=p.atencion_id AND c.usuario_id=@UsuarioId))
        """;

    private const string PropiedadOrden = """
        (@Rol IN ('GERENTE','EMPLEADO') OR EXISTS (SELECT 1 FROM presupuestos p JOIN atenciones a ON a.id=p.atencion_id
            JOIN clientes c ON c.id=a.cliente_id WHERE p.id=o.presupuesto_id AND c.usuario_id=@UsuarioId))
        """;

    /// <summary>Abre una conexión para una operación corta del taller.</summary>
    private Task<MySqlConnection> Abrir(CancellationToken ct) => baseDatos.AbrirAsync(ct);

    /// <summary>Guarda la acción junto con el cambio que la generó.</summary>
    private static Task Auditar(MySqlConnection db, MySqlTransaction tx, ActorTaller actor,
        string entidad, ulong id, string accion, CancellationToken ct, object? antes = null, object? despues = null) =>
        AuditoriaSql.RegistrarAsync(db, tx, actor.UsuarioId, entidad, id, accion,
            antes is null ? null : JsonSerializer.Serialize(antes),
            despues is null ? null : JsonSerializer.Serialize(despues), ct);

    /// <summary>Bloquea primero la orden para serializar trabajo, piezas y resolución de cancelaciones.</summary>
    private static async Task<OrdenBloqueada?> BloquearOrden(MySqlConnection db, MySqlTransaction tx,
        ulong id, CancellationToken ct)
    {
        if (await db.ExecuteScalarAsync<ulong?>(new CommandDefinition(
                "SELECT id FROM ordenes_servicio WHERE id = @id FOR UPDATE", new { id }, tx,
                cancellationToken: ct)) is null)
            return null;
        return await db.QuerySingleAsync<OrdenBloqueada>(new CommandDefinition("""
            SELECT o.id AS Id, o.presupuesto_id AS PresupuestoId, o.estado AS Estado, a.id AS AtencionId,
                a.cliente_id AS ClienteId, c.usuario_id AS TitularUsuarioId, e.usuario_id AS MecanicoUsuarioId,
                p.mano_obra_acordada AS ManoObra, p.refacciones_acordadas AS Refacciones,
                ap.total_pagado AS Pagado, o.aviso_avance_25_en AS AvisoAvance
            FROM ordenes_servicio o JOIN presupuestos p ON p.id = o.presupuesto_id
            JOIN atenciones a ON a.id = p.atencion_id JOIN clientes c ON c.id = a.cliente_id
            JOIN empleados e ON e.id = a.mecanico_id
            LEFT JOIN anticipos_reparacion ap ON ap.orden_id = o.id WHERE o.id = @id
            """, new { id }, tx, cancellationToken: ct));
    }

    /// <summary>Permite registrar trabajo al responsable actual o a gerencia.</summary>
    private static void ExigirResponsable(OrdenBloqueada orden, ActorTaller actor)
    {
        if (actor.Rol != "GERENTE" && actor.UsuarioId != orden.MecanicoUsuarioId)
            throw new UnauthorizedAccessException("Solo el mecánico asignado o gerencia registra el trabajo.");
    }

    /// <summary>Devuelve la evaluación de progreso a partir de todas las tareas persistidas.</summary>
    private static decimal Avance(IEnumerable<TareaVista> tareas) =>
        CalculadoraAvance.Calcular(tareas.Select(t => (t.Peso, t.Porcentaje)));

    /// <summary>Consulta las tareas en el orden acordado en el presupuesto.</summary>
    private static async Task<IReadOnlyList<TareaVista>> Tareas(MySqlConnection db, MySqlTransaction tx,
        ulong presupuestoId, CancellationToken ct) =>
        (await db.QueryAsync<TareaVista>(new CommandDefinition("""
            SELECT id AS Id, descripcion AS Descripcion, peso_porcentual AS Peso, avance_tarea AS Porcentaje,
                estado AS Estado, actualizada_en AS ActualizadaEn
            FROM tareas_servicio WHERE presupuesto_id = @presupuestoId ORDER BY orden
            """, new { presupuestoId }, tx, cancellationToken: ct))).AsList();

    /// <summary>Consulta las piezas acordadas sin exponer la factura ni costos del proveedor.</summary>
    private static async Task<IReadOnlyList<RefaccionVista>> Piezas(MySqlConnection db, MySqlTransaction tx,
        ulong presupuestoId, CancellationToken ct) =>
        (await db.QueryAsync<RefaccionVista>(new CommandDefinition("""
            SELECT id AS Id, descripcion AS Descripcion, cantidad_solicitada AS Solicitadas,
                cantidad_recibida AS Recibidas, cantidad_utilizada AS Utilizadas, cantidad_devuelta AS Devueltas,
                precio_unitario_cliente AS PrecioUnitario, solicitada_en AS SolicitadaEn
            FROM refacciones_servicio WHERE presupuesto_id = @presupuestoId ORDER BY id
            """, new { presupuestoId }, tx, cancellationToken: ct))).AsList();

    /// <summary>Agrega un aviso privado único para la orden y el tipo de evento registrado.</summary>
    private static Task NotificarOrden(MySqlConnection db, MySqlTransaction tx, OrdenBloqueada orden,
        string tipo, string mensaje, CancellationToken ct) =>
        db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO notificaciones (cliente_id,orden_id,tipo,mensaje)
            VALUES (@ClienteId,@Id,@tipo,@mensaje) ON DUPLICATE KEY UPDATE id = notificaciones.id
            """, new { orden.ClienteId, orden.Id, tipo, mensaje }, tx, cancellationToken: ct));

    /// <summary>Datos que se mantienen coherentes mientras se conserva el bloqueo sobre la orden.</summary>
    private sealed record OrdenBloqueada(ulong Id, ulong PresupuestoId, string Estado, ulong AtencionId,
        ulong ClienteId, ulong TitularUsuarioId, ulong MecanicoUsuarioId, decimal ManoObra, decimal Refacciones,
        decimal? Pagado, DateTime? AvisoAvance)
    {
        public bool Abierta => Estado is "LISTA" or "EN_EJECUCION" or "PENDIENTE_PIEZAS";
        public bool PuedeEjecutarse => Pagado.HasValue && Estado is "LISTA" or "EN_EJECUCION";
    }
}
