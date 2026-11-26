namespace FollowMotors.Infrastructure.Operaciones;

using System.Data;
using Dapper;
using FollowMotors.Application.Operaciones;
using FollowMotors.Infrastructure.Database;

/// <summary>Guarda cotización y movimientos de piezas sin cambiar el importe acordado al cliente.</summary>
public sealed partial class OperacionesTallerMySql
{
    /// <summary>Agrega una pieza al desglose de un presupuesto que aún se puede modificar.</summary>
    public async Task<ulong> SolicitarRefaccion(SolicitarRefaccionCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var presupuesto = await BloquearPresupuesto(db, tx, solicitud.PresupuestoId, ct);
        if (presupuesto.Estado != "BORRADOR")
            throw new InvalidOperationException("El presupuesto ya fue decidido.");
        var piezas = await Piezas(db, tx, presupuesto.Id, ct);
        decimal cotizado = piezas.Sum(p => p.Solicitadas * p.PrecioUnitario);
        if (cotizado + solicitud.Cantidad * solicitud.PrecioUnitario > presupuesto.Refacciones)
            throw new InvalidOperationException("El desglose supera el importe de refacciones de la cotización.");
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO refacciones_servicio (presupuesto_id,descripcion,cantidad_solicitada,precio_unitario_cliente)
            VALUES (@PresupuestoId,@descripcion,@Cantidad,@PrecioUnitario)
            """, new { solicitud.PresupuestoId, descripcion = solicitud.Descripcion.Trim(),
                solicitud.Cantidad, solicitud.PrecioUnitario }, tx, cancellationToken: ct));
        ulong id = await ConsultasMySql.IdNuevoAsync(db, tx, ct);
        await Auditar(db, tx, actor, "refacciones_servicio", id, "COTIZAR", ct, despues: solicitud);
        await tx.CommitAsync(ct);
        return id;
    }

    /// <summary>Marca como realizada la solicitud al proveedor para una reparación pagada.</summary>
    public Task<bool> PedirRefaccion(ulong id, ActorTaller actor, CancellationToken ct) =>
        MoverRefaccion(id, 0, null, null, Movimiento.Pedir, actor, ct);

    /// <summary>Registra la recepción parcial o total de unidades pedidas.</summary>
    public Task<bool> RecibirRefaccion(RecibirRefaccionCommand solicitud, ActorTaller actor, CancellationToken ct) =>
        MoverRefaccion(solicitud.RefaccionId, solicitud.Cantidad, solicitud.CostoUnitario,
            solicitud.Factura, Movimiento.Recibir, actor, ct);

    /// <summary>Registra el consumo comprobado de una pieza por el responsable del trabajo.</summary>
    public Task<bool> ConsumirRefaccion(ConsumirRefaccionCommand solicitud, ActorTaller actor, CancellationToken ct) =>
        MoverRefaccion(solicitud.RefaccionId, solicitud.Cantidad, null, null, Movimiento.Usar, actor, ct);

    /// <summary>Asienta el retorno de unidades selladas de una orden detenida o finalizada.</summary>
    public Task<bool> DevolverRefaccion(DevolverRefaccionCommand solicitud, ActorTaller actor, CancellationToken ct)
    {
        if (!solicitud.Sellada) throw new ArgumentException("Confirma que la refacción a devolver sigue sellada.");
        return MoverRefaccion(solicitud.RefaccionId, solicitud.Cantidad, null, null, Movimiento.Devolver, actor, ct);
    }

    /// <summary>Aplica el movimiento y actualiza exclusivamente la orden de la pieza afectada.</summary>
    private async Task<bool> MoverRefaccion(ulong id, int cantidad, decimal? costo, string? factura, Movimiento movimiento,
        ActorTaller actor, CancellationToken ct)
    {
        if (movimiento != Movimiento.Pedir && cantidad is < 1 or > ushort.MaxValue)
            throw new ArgumentException("Indica una cantidad de refacciones válida.");
        await using var db = await Abrir(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        ulong? ordenId = await db.ExecuteScalarAsync<ulong?>(new CommandDefinition("""
            SELECT o.id FROM ordenes_servicio o JOIN refacciones_servicio r ON r.presupuesto_id = o.presupuesto_id
            WHERE r.id = @id
            """, new { id }, tx, cancellationToken: ct));
        if (ordenId is null) return false;
        var orden = await BloquearOrden(db, tx, ordenId.Value, ct);
        if (orden is null || orden.Pagado is null) return false;
        var clausula = movimiento switch
        {
            Movimiento.Pedir when orden.Abierta =>
                "solicitada_en = UTC_TIMESTAMP(3) WHERE id = @id AND solicitada_en IS NULL",
            Movimiento.Recibir when orden.Abierta => """
                cantidad_recibida = cantidad_recibida + @cantidad, costo_unitario_proveedor = @costo,
                referencia_factura_proveedor = @factura, recibida_en = UTC_TIMESTAMP(3)
                WHERE id = @id AND solicitada_en IS NOT NULL AND cantidad_recibida + @cantidad <= cantidad_solicitada
                """,
            Movimiento.Usar when orden.PuedeEjecutarse => """
                cantidad_utilizada = cantidad_utilizada + @cantidad
                WHERE id = @id AND cantidad_utilizada + cantidad_devuelta + @cantidad <= cantidad_recibida
                """,
            Movimiento.Devolver when orden.Estado is "TERMINADA" or "CANCELADA" or "ENTREGADA" => """
                cantidad_devuelta = cantidad_devuelta + @cantidad, devuelta_en = UTC_TIMESTAMP(3)
                WHERE id = @id AND cantidad_utilizada + cantidad_devuelta + @cantidad <= cantidad_recibida
                """,
            _ => null
        };
        if (clausula is null) return false;
        if (movimiento == Movimiento.Usar) ExigirResponsable(orden, actor);
        int filas = await db.ExecuteAsync(new CommandDefinition("UPDATE refacciones_servicio SET " + clausula,
            new { id, cantidad, costo, factura }, tx, cancellationToken: ct));
        if (filas != 1) return false;
        if (movimiento == Movimiento.Recibir && orden.Estado == "PENDIENTE_PIEZAS"
            && (await Piezas(db, tx, orden.PresupuestoId, ct)).All(p => p.Recibidas == p.Solicitadas))
            await db.ExecuteAsync(new CommandDefinition("""
                UPDATE ordenes_servicio SET estado = 'LISTA', version = version + 1 WHERE id = @Id
                """, new { orden.Id }, tx, cancellationToken: ct));
        await Auditar(db, tx, actor, "refacciones_servicio", id, movimiento.ToString().ToUpperInvariant(), ct,
            despues: new { cantidad, costo, factura });
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>Movimientos aplicables a una pieza durante la vida de la reparación.</summary>
    private enum Movimiento { Pedir, Recibir, Usar, Devolver }
}
