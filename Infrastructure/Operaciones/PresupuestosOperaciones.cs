namespace FollowMotors.Infrastructure.Operaciones;

using Dapper;
using FollowMotors.Application.Operaciones;

/// <summary>Persistencia de presupuestos, contratación y anticipos con transacciones atómicas.</summary>
public sealed partial class OperacionesTallerMySql
{
    /// <summary>Guarda un presupuesto y su plan de tareas como una única operación.</summary>
    public async Task<ulong> CrearPresupuesto(CrearPresupuestoCommand c, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        var filas = await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO presupuestos(atencion_id,tipo_servicio_id,descripcion_trabajo,mano_obra_acordada,
                refacciones_acordadas,entrega_estimada,registrado_por_usuario_id)
            SELECT a.id,t.id,@Descripcion,@ManoObra,@Refacciones,@EntregaEstimada,@UsuarioId
            FROM atenciones a JOIN tipos_servicio t ON t.id=@TipoServicioId AND t.activo=1
            WHERE a.id=@AtencionId AND a.estado='DIAGNOSTICADA'
            """, new {c.AtencionId,c.TipoServicioId,c.Descripcion,c.ManoObra,c.Refacciones,
                c.EntregaEstimada,actor.UsuarioId},tx,cancellationToken:ct));
        if (filas != 1) throw new InvalidOperationException("La atención no está diagnosticada o el servicio no está disponible.");
        var id = await IdNuevo(db,tx,ct);
        for (var i=0;i<c.Tareas.Count;i++)
            await db.ExecuteAsync(new CommandDefinition("""
                INSERT INTO tareas_servicio(presupuesto_id,descripcion,orden,peso_porcentual)
                VALUES(@id,@Descripcion,@orden,@Peso)
                """,new {id,c.Tareas[i].Descripcion,orden=i+1,c.Tareas[i].Peso},tx,cancellationToken:ct));
        await Auditar(db,tx,actor,"presupuestos",id,"CREAR",ct);
        await tx.CommitAsync(ct);
        return id;
    }

    /// <summary>Establece una decisión irreversible sobre el precio acordado.</summary>
    public async Task<bool> DecidirPresupuesto(DecidirPresupuestoCommand c, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            UPDATE presupuestos SET estado=@estado, acordado_en=UTC_TIMESTAMP(3)
            WHERE id=@PresupuestoId AND estado='BORRADOR'
            """,new {c.PresupuestoId,estado=c.Aceptar?"ACEPTADO":"RECHAZADO"},tx,cancellationToken:ct));
        if(n==1) await Auditar(db,tx,actor,"presupuestos",c.PresupuestoId,c.Aceptar?"ACEPTAR":"RECHAZAR",ct);
        await tx.CommitAsync(ct);
        return n==1;
    }

    /// <summary>Obtiene un presupuesto, sin revelar expedientes de otros clientes.</summary>
    public async Task<PresupuestoVista?> ConsultarPresupuesto(ulong id, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        return await db.QuerySingleOrDefaultAsync<PresupuestoVista>(new CommandDefinition("""
            SELECT p.id AS Id,p.atencion_id AS AtencionId,p.descripcion_trabajo AS DescripcionTrabajo,
            p.mano_obra_acordada AS ManoObra,p.refacciones_acordadas AS Refacciones,
            p.total_acordado AS Total,p.estado AS Estado,p.entrega_estimada AS EntregaEstimada
            FROM presupuestos p WHERE p.id=@id AND 
            """+PropiedadPresupuesto,
            new {id,actor.Rol,actor.UsuarioId},cancellationToken:ct));
    }

    /// <summary>Contrata el presupuesto aceptado y registra simultáneamente su garantía informada.</summary>
    public async Task<ulong> Contratar(ulong presupuestoId, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO ordenes_servicio(presupuesto_id,estado)
            SELECT p.id,CASE WHEN EXISTS
             (SELECT 1 FROM refacciones_servicio r WHERE r.presupuesto_id=p.id
                 AND r.cantidad_recibida<r.cantidad_solicitada)
             THEN 'PENDIENTE_PIEZAS' ELSE 'LISTA' END
            FROM presupuestos p JOIN atenciones a ON a.id=p.atencion_id
            WHERE p.id=@presupuestoId AND p.estado='ACEPTADO' AND a.estado='DIAGNOSTICADA'
            """,new {presupuestoId},tx,cancellationToken:ct));
        if(n!=1) throw new InvalidOperationException("El presupuesto no está aceptado o ya fue contratado.");
        var id=await IdNuevo(db,tx,ct);
        await db.ExecuteAsync(new CommandDefinition("""
            UPDATE atenciones a JOIN presupuestos p ON p.atencion_id=a.id
            SET a.estado='CONTRATADA' WHERE p.id=@presupuestoId
            """,new {presupuestoId},tx,cancellationToken:ct));
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO garantias_servicio(orden_id,aplica,duracion_dias,condiciones,informada_en)
            SELECT @id,(t.garantia_dias>0),t.garantia_dias,COALESCE(t.cobertura,'Sin cobertura especificada'),
                   UTC_TIMESTAMP(3)
            FROM presupuestos p JOIN tipos_servicio t ON t.id=p.tipo_servicio_id
            WHERE p.id=@presupuestoId
            """,new {id,presupuestoId},tx,cancellationToken:ct));
        await Auditar(db,tx,actor,"ordenes_servicio",id,"CONTRATAR",ct);
        await tx.CommitAsync(ct);
        return id;
    }

    /// <summary>Registra un único anticipo con límites separados por concepto.</summary>
    public async Task<ulong> RegistrarAnticipo(RegistrarAnticipoCommand c, ActorTaller actor, CancellationToken ct)
    {
        await using var db=await Abrir(ct);
        await using var tx=await db.BeginTransactionAsync(ct);
        var n=await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO anticipos_reparacion(orden_id,monto_mano_obra,monto_refacciones,
                referencia_comprobante,registrado_por_usuario_id)
            SELECT o.id,@ManoObra,@Refacciones,@Comprobante,@UsuarioId
            FROM ordenes_servicio o JOIN presupuestos p ON p.id=o.presupuesto_id
            WHERE o.id=@OrdenId AND o.estado NOT IN('CANCELADA','ENTREGADA')
              AND @ManoObra<=p.mano_obra_acordada AND @Refacciones<=p.refacciones_acordadas
            """,new {c.OrdenId,c.ManoObra,c.Refacciones,c.Comprobante,actor.UsuarioId},tx,cancellationToken:ct));
        if(n!=1) throw new InvalidOperationException("Anticipo inválido, duplicado o mayor al importe acordado.");
        var id=await IdNuevo(db,tx,ct);
        await Auditar(db,tx,actor,"anticipos_reparacion",id,"COBRAR_ANTICIPO",ct);
        await tx.CommitAsync(ct);
        return id;
    }
}
