namespace FollowMotors.Infrastructure.Operaciones;

using Dapper;
using FollowMotors.Application.Operaciones;

/// <summary>Actualización de mecánicos, tareas y refacciones con restricciones de estado.</summary>
public sealed partial class OperacionesTallerMySql
{
    /// <summary>Reasigna un mecánico solamente si permanece activo y disponible.</summary>
    public async Task<bool> AsignarMecanico(AsignarMecanicoCommand c, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            UPDATE atenciones a JOIN presupuestos p ON p.atencion_id=a.id
            JOIN ordenes_servicio o ON o.presupuesto_id=p.id
            JOIN empleados e ON e.id=@MecanicoId AND e.es_mecanico=1 AND e.disponible=1
            JOIN usuarios u ON u.id=e.usuario_id AND u.activo=1
            SET a.mecanico_id=e.id
            WHERE o.id=@OrdenId AND o.estado IN('LISTA','PENDIENTE_PIEZAS','EN_EJECUCION')
            """,new {c.OrdenId,c.MecanicoId},tx,cancellationToken:ct));
        if(n==1) await Auditar(db,tx,actor,"ordenes_servicio",c.OrdenId,"ASIGNAR_MECANICO",ct);
        await tx.CommitAsync(ct);
        return n>0;
    }

    /// <summary>Actualiza una tarea que pertenezca a la orden y evita retrocesos accidentales.</summary>
    public async Task<bool> AvanzarTarea(ActualizarTareaCommand c, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            UPDATE tareas_servicio t
            JOIN ordenes_servicio o ON o.presupuesto_id=t.presupuesto_id
            SET t.avance_tarea=@Porcentaje,t.evidencia=@Evidencia,
                t.estado=CASE WHEN @Porcentaje=100 THEN 'COMPLETADA'
                  WHEN @Porcentaje=0 THEN 'PENDIENTE' ELSE 'EN_PROCESO' END,
                t.actualizada_en=UTC_TIMESTAMP(3),
                o.estado='EN_EJECUCION',o.iniciada_en=COALESCE(o.iniciada_en,UTC_TIMESTAMP(3)),
                o.version=o.version+1
            WHERE t.id=@TareaId AND o.id=@OrdenId
              AND o.estado IN('LISTA','EN_EJECUCION')
              AND @Porcentaje>=t.avance_tarea
            """,new {c.OrdenId,c.TareaId,c.Porcentaje,c.Evidencia},tx,cancellationToken:ct));
        if(n>0)
        {
            await db.ExecuteAsync(new CommandDefinition("""
                INSERT IGNORE INTO notificaciones(cliente_id,orden_id,tipo,mensaje)
                SELECT a.cliente_id,o.id,'AVANCE_25','Tu servicio alcanzó al menos el 25 % de avance.'
                FROM ordenes_servicio o JOIN presupuestos p ON p.id=o.presupuesto_id
                JOIN atenciones a ON a.id=p.atencion_id
                WHERE o.id=@OrdenId AND (SELECT SUM(t.peso_porcentual*t.avance_tarea/100)
                    FROM tareas_servicio t WHERE t.presupuesto_id=p.id)>=25
                """,new {c.OrdenId},tx,cancellationToken:ct));
            await Auditar(db,tx,actor,"tareas_servicio",c.TareaId,"ACTUALIZAR_AVANCE",ct);
        }
        await tx.CommitAsync(ct);
        return n>0;
    }

    /// <summary>Cierra técnicamente solo órdenes con tareas terminadas y materiales contabilizados.</summary>
    public async Task<bool> Finalizar(ulong ordenId, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            UPDATE ordenes_servicio o JOIN presupuestos p ON p.id=o.presupuesto_id
            SET o.estado='TERMINADA',o.terminada_en=UTC_TIMESTAMP(3),o.version=o.version+1
            WHERE o.id=@ordenId AND o.estado IN('LISTA','EN_EJECUCION')
            AND NOT EXISTS(SELECT 1 FROM tareas_servicio t
                WHERE t.presupuesto_id=p.id AND t.avance_tarea<100)
            AND NOT EXISTS(SELECT 1 FROM refacciones_servicio r
                WHERE r.presupuesto_id=p.id AND r.cantidad_recibida<r.cantidad_solicitada)
            AND NOT EXISTS(SELECT 1 FROM solicitudes_cancelacion s
                WHERE s.orden_id=o.id AND s.estado='SOLICITADA')
            """,new {ordenId},tx,cancellationToken:ct));
        if(n==1)
        {
            await db.ExecuteAsync(new CommandDefinition("""
                INSERT IGNORE INTO notificaciones(cliente_id,orden_id,tipo,mensaje)
                SELECT a.cliente_id,o.id,'SERVICIO_TERMINADO','Tu motocicleta está lista para entrega.'
                FROM ordenes_servicio o JOIN presupuestos p ON p.id=o.presupuesto_id
                JOIN atenciones a ON a.id=p.atencion_id WHERE o.id=@ordenId
                """,new {ordenId},tx,cancellationToken:ct));
            await Auditar(db,tx,actor,"ordenes_servicio",ordenId,"FINALIZAR",ct);
        }
        await tx.CommitAsync(ct);
        return n==1;
    }

    /// <summary>Confirma entrega y fija las fechas de cobertura de garantía sin crear una segunda.</summary>
    public async Task<bool> Entregar(EntregarOrdenCommand c, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            UPDATE ordenes_servicio o JOIN presupuestos p ON p.id=o.presupuesto_id
            JOIN atenciones a ON a.id=p.atencion_id
            SET o.estado='ENTREGADA',o.entregada_en=UTC_TIMESTAMP(3),o.version=o.version+1,
                a.estado='CERRADA'
            WHERE o.id=@OrdenId AND o.estado='TERMINADA'
            """,new {c.OrdenId},tx,cancellationToken:ct));
        if(n==1)
        {
            await db.ExecuteAsync(new CommandDefinition("""
                UPDATE garantias_servicio g SET g.inicia_en=UTC_TIMESTAMP(3),
                g.vence_en=DATE_ADD(UTC_TIMESTAMP(3),INTERVAL g.duracion_dias DAY),
                g.referencia_comprobante=@Comprobante
                WHERE g.orden_id=@OrdenId AND g.inicia_en IS NULL
                """,new {c.OrdenId,c.Comprobante},tx,cancellationToken:ct));
            await Auditar(db,tx,actor,"ordenes_servicio",c.OrdenId,"ENTREGAR",ct);
        }
        await tx.CommitAsync(ct);
        return n==1;
    }

    /// <summary>Calcula el progreso ponderado de las tareas desde datos almacenados.</summary>
    public async Task<OrdenVista?> ConsultarOrden(ulong id, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        return await db.QuerySingleOrDefaultAsync<OrdenVista>(new CommandDefinition("""
            SELECT o.id AS Id,o.presupuesto_id AS PresupuestoId,o.estado AS Estado,
            COALESCE((SELECT SUM(t.peso_porcentual*t.avance_tarea/100)
                FROM tareas_servicio t WHERE t.presupuesto_id=o.presupuesto_id),0) AS Avance,
            o.terminada_en AS TerminadaEn,o.entregada_en AS EntregadaEn
            FROM ordenes_servicio o WHERE o.id=@id AND
            """+PropiedadOrden,new {id,actor.Rol,actor.UsuarioId},cancellationToken:ct));
    }

    /// <summary>Solicita una pieza solo mientras el presupuesto está en borrador.</summary>
    public async Task<ulong> SolicitarRefaccion(SolicitarRefaccionCommand c, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO refacciones_servicio(presupuesto_id,descripcion,cantidad_solicitada,
                precio_unitario_cliente,solicitada_en)
            SELECT p.id,@Descripcion,@Cantidad,@PrecioUnitario,UTC_TIMESTAMP(3)
            FROM presupuestos p WHERE p.id=@PresupuestoId AND p.estado='BORRADOR'
            """,new {c.PresupuestoId,c.Descripcion,c.Cantidad,c.PrecioUnitario},tx,cancellationToken:ct));
        if(n!=1) throw new InvalidOperationException("La refacción requiere un presupuesto en borrador.");
        var id=await IdNuevo(db,tx,ct);
        await Auditar(db,tx,actor,"refacciones_servicio",id,"SOLICITAR",ct);
        await tx.CommitAsync(ct);
        return id;
    }

    /// <summary>Recibe unidades pendientes sin rebasar lo solicitado ni cerrar órdenes.</summary>
    public async Task<bool> RecibirRefaccion(RecibirRefaccionCommand c, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            UPDATE refacciones_servicio r
            JOIN ordenes_servicio o ON o.presupuesto_id=r.presupuesto_id
            SET r.cantidad_recibida=r.cantidad_recibida+@Cantidad,
                r.costo_unitario_proveedor=@CostoUnitario,
                r.referencia_factura_proveedor=@Factura,r.recibida_en=UTC_TIMESTAMP(3)
            WHERE r.id=@RefaccionId AND o.estado IN('PENDIENTE_PIEZAS','LISTA','EN_EJECUCION')
            AND r.cantidad_recibida+@Cantidad<=r.cantidad_solicitada
            """,new {c.RefaccionId,c.Cantidad,c.CostoUnitario,c.Factura},tx,cancellationToken:ct));
        if(n==1)
        {
            await db.ExecuteAsync(new CommandDefinition("""
                UPDATE ordenes_servicio o SET o.estado='LISTA'
                WHERE o.estado='PENDIENTE_PIEZAS' AND NOT EXISTS
                  (SELECT 1 FROM refacciones_servicio r WHERE r.presupuesto_id=o.presupuesto_id
                   AND r.cantidad_recibida<r.cantidad_solicitada)
                """,transaction:tx,cancellationToken:ct));
            await Auditar(db,tx,actor,"refacciones_servicio",c.RefaccionId,"RECIBIR",ct);
        }
        await tx.CommitAsync(ct);
        return n==1;
    }

    /// <summary>Consume inventario recibido, sin permitir cantidades negativas disponibles.</summary>
    public Task<bool> ConsumirRefaccion(ConsumirRefaccionCommand c, ActorTaller actor, CancellationToken ct) =>
        MoverRefaccion(c.RefaccionId,c.Cantidad,actor,"USAR",ct);

    /// <summary>Devuelve sobrantes de material recibido que no han sido utilizados.</summary>
    public Task<bool> DevolverRefaccion(DevolverRefaccionCommand c, ActorTaller actor, CancellationToken ct) =>
        MoverRefaccion(c.RefaccionId,c.Cantidad,actor,"DEVOLVER",ct);

    /// <summary>Unifica los dos movimientos con el mismo control de disponibilidad.</summary>
    private async Task<bool> MoverRefaccion(ulong id,int cantidad,ActorTaller actor,string accion,CancellationToken ct)
    {
        var columna=accion=="USAR"?"cantidad_utilizada":"cantidad_devuelta";
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var sql=$"""
            UPDATE refacciones_servicio r JOIN ordenes_servicio o ON o.presupuesto_id=r.presupuesto_id
            SET r.{columna}=r.{columna}+@cantidad,
                r.devuelta_en=CASE WHEN @accion='DEVOLVER' THEN UTC_TIMESTAMP(3) ELSE r.devuelta_en END
            WHERE r.id=@id AND o.estado IN('LISTA','EN_EJECUCION','TERMINADA')
              AND r.cantidad_utilizada+r.cantidad_devuelta+@cantidad<=r.cantidad_recibida
            """;
        var n=await db.ExecuteAsync(new CommandDefinition(sql,new {id,cantidad,accion},tx,cancellationToken:ct));
        if(n==1) await Auditar(db,tx,actor,"refacciones_servicio",id,accion,ct);
        await tx.CommitAsync(ct);
        return n==1;
    }
}
