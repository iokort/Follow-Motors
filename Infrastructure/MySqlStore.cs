using System.Data.Common;
using FollowMotors.Application;
using FollowMotors.Domain;
using MySqlConnector;

namespace FollowMotors.Infrastructure;

// La API usa exclusivamente SQL parametrizado; no concatena datos de entrada.
public sealed class MySqlStore(MySqlDataSource pool) :
    IUsuarioQueries, IClientesStore, IMotocicletasStore, ICitasStore,
    IServiciosStore, IReportesQueries, IAvisosStore
{
    private static DbCommand Command(DbConnection db, string sql, DbTransaction? tx = null, params object?[] args)
    {
        var cmd = db.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = tx;
        for (var i = 0; i < args.Length; i++)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = "@p" + i;
            p.Value = args[i] ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
        return cmd;
    }
    private async Task<IReadOnlyList<T>> List<T>(string sql, Func<DbDataReader, T> map, CancellationToken ct, params object?[] args)
    {
        await using var db = await pool.OpenConnectionAsync(ct);
        await using var cmd = Command(db, sql, null, args);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        var rows = new List<T>();
        while (await r.ReadAsync(ct)) rows.Add(map(r));
        return rows;
    }
    private async Task<object?> Scalar(string sql, CancellationToken ct, params object?[] args)
    {
        await using var db = await pool.OpenConnectionAsync(ct);
        await using var cmd = Command(db, sql, null, args);
        return await cmd.ExecuteScalarAsync(ct);
    }
    private async Task<int> Update(string sql, CancellationToken ct, params object?[] args)
    {
        await using var db = await pool.OpenConnectionAsync(ct);
        await using var cmd = Command(db, sql, null, args);
        return await cmd.ExecuteNonQueryAsync(ct);
    }
    private async Task<long> Insert(string sql, CancellationToken ct, params object?[] args)
    {
        await using var db = await pool.OpenConnectionAsync(ct);
        await using (var cmd = Command(db, sql, null, args))
            await cmd.ExecuteNonQueryAsync(ct);
        await using var id = Command(db, "SELECT LAST_INSERT_ID()");
        return Convert.ToInt64(await id.ExecuteScalarAsync(ct));
    }

    public async Task<Usuario?> BuscarPorIdentificadorAsync(string identificador, CancellationToken ct) =>
        (await List("SELECT id, identificador, hash_contrasena, rol, activo FROM usuarios WHERE identificador=@p0 LIMIT 1",
            r => new Usuario(r.GetInt64(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetBoolean(4)),
            ct, identificador)).FirstOrDefault();

    public async Task<bool> ActivoAsync(long usuarioId, CancellationToken ct) =>
        Convert.ToInt32(await Scalar("SELECT COUNT(*) FROM usuarios WHERE id=@p0 AND activo=1", ct, usuarioId)) == 1;

    public Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken ct) =>
        List("SELECT id,usuario_id,nombres,apellidos,telefono FROM clientes ORDER BY id DESC",
            r => new Cliente(r.GetInt64(0), r.GetInt64(1), r.GetString(2), r.GetString(3), r.IsDBNull(4) ? null : r.GetString(4)), ct);

    public async Task<Cliente?> ObtenerAsync(long id, CancellationToken ct) =>
        (await List("SELECT id,usuario_id,nombres,apellidos,telefono FROM clientes WHERE id=@p0",
            r => new Cliente(r.GetInt64(0), r.GetInt64(1), r.GetString(2), r.GetString(3), r.IsDBNull(4) ? null : r.GetString(4)), ct, id)).FirstOrDefault();

    public async Task<long?> BuscarIdPorUsuarioAsync(long usuarioId, CancellationToken ct)
    {
        var value = await Scalar("SELECT id FROM clientes WHERE usuario_id=@p0", ct, usuarioId);
        return value is null or DBNull ? null : Convert.ToInt64(value);
    }
    public Task<long> CrearAsync(CrearClienteCommand cmd, long operadorId, CancellationToken ct) =>
        Insert("INSERT INTO clientes(usuario_id,nombres,apellidos,telefono,registrado_por_usuario_id) VALUES(@p0,@p1,@p2,@p3,@p4)",
            ct, cmd.UsuarioId, cmd.Nombres.Trim(), cmd.Apellidos.Trim(), cmd.Telefono, operadorId);

    public async Task<bool> ActualizarAsync(long id, ActualizarClienteCommand cmd, CancellationToken ct) =>
        await Update("UPDATE clientes SET nombres=@p0,apellidos=@p1,telefono=@p2 WHERE id=@p3", ct,
            cmd.Nombres.Trim(), cmd.Apellidos.Trim(), cmd.Telefono, id) > 0;

    Task<IReadOnlyList<Motocicleta>> IMotocicletasStore.ListarAsync(long? clienteId, CancellationToken ct) =>
        List("SELECT id,cliente_id,numero_serie,marca,modelo,anio,kilometraje_actual FROM motocicletas WHERE (@p0 IS NULL OR cliente_id=@p0) ORDER BY id DESC",
            r => new Motocicleta(r.GetInt64(0), r.GetInt64(1), r.GetString(2), r.GetString(3),
                r.GetString(4), r.IsDBNull(5) ? null : Convert.ToInt32(r.GetValue(5)), Convert.ToInt32(r.GetValue(6))), ct, clienteId);

    public async Task<long?> BuscarClienteAsync(long motoId, CancellationToken ct)
    {
        var value = await Scalar("SELECT cliente_id FROM motocicletas WHERE id=@p0", ct, motoId);
        return value is null or DBNull ? null : Convert.ToInt64(value);
    }
    public Task<long> CrearAsync(RegistrarMotocicletaCommand cmd, CancellationToken ct) =>
        Insert("INSERT INTO motocicletas(cliente_id,numero_serie,marca,modelo,anio,kilometraje_actual) VALUES(@p0,@p1,@p2,@p3,@p4,@p5)",
            ct, cmd.ClienteId, cmd.NumeroSerie.Trim(), cmd.Marca.Trim(), cmd.Modelo.Trim(), cmd.Anio, cmd.KilometrajeActual);

    public async Task<bool> ActualizarAsync(long id, ActualizarMotocicletaCommand cmd, CancellationToken ct) =>
        await Update("UPDATE motocicletas SET marca=@p0,modelo=@p1,anio=@p2,kilometraje_actual=@p3 WHERE id=@p4",
            ct, cmd.Marca.Trim(), cmd.Modelo.Trim(), cmd.Anio, cmd.KilometrajeActual, id) > 0;

    Task<IReadOnlyList<Cita>> ICitasStore.ListarAsync(long? clienteId, CancellationToken ct) =>
        List("SELECT c.id,c.franja_id,c.cliente_id,c.motocicleta_id,c.motivo,c.estado,f.inicio FROM citas c JOIN franjas_citas f ON f.id=c.franja_id WHERE (@p0 IS NULL OR c.cliente_id=@p0) ORDER BY f.inicio DESC",
            r => new Cita(r.GetInt64(0), r.GetInt64(1), r.GetInt64(2),
                r.IsDBNull(3) ? null : r.GetInt64(3), r.GetString(4), r.GetString(5), r.GetDateTime(6)), ct, clienteId);

    public async Task<long> ReservarAsync(ReservarCitaCommand cmd, string origen, CancellationToken ct)
    {
        await using var db = await pool.OpenConnectionAsync(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        await using (var slot = Command(db, "SELECT capacidad FROM franjas_citas WHERE id=@p0 AND habilitada=1 FOR UPDATE", tx, cmd.FranjaId))
        {
            var limit = await slot.ExecuteScalarAsync(ct);
            if (limit is null) throw new InvalidOperationException("Franja inexistente o inhabilitada.");
            await using var used = Command(db, "SELECT COUNT(*) FROM citas WHERE franja_id=@p0 AND estado='CONFIRMADA'", tx, cmd.FranjaId);
            var count = Convert.ToInt32(await used.ExecuteScalarAsync(ct));
            if (count >= Convert.ToInt32(limit)) throw new InvalidOperationException("La franja no tiene cupo.");
        }
        await using (var ins = Command(db,
            "INSERT INTO citas(franja_id,cliente_id,motocicleta_id,motivo,origen) VALUES(@p0,@p1,@p2,@p3,@p4)",
            tx, cmd.FranjaId, cmd.ClienteId, cmd.MotocicletaId, cmd.Motivo.Trim(), origen))
            await ins.ExecuteNonQueryAsync(ct);
        await using var id = Command(db, "SELECT LAST_INSERT_ID()", tx);
        var result = Convert.ToInt64(await id.ExecuteScalarAsync(ct));
        await tx.CommitAsync(ct);
        return result;
    }

    public async Task<bool> ReprogramarAsync(long id, ReprogramarCitaCommand cmd, long? clienteId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.Motivo)) throw new ArgumentException("El motivo es obligatorio.");
        await using var db = await pool.OpenConnectionAsync(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        await using var current = Command(db,
            "SELECT cliente_id FROM citas WHERE id=@p0 AND estado='CONFIRMADA' FOR UPDATE", tx, id);
        var owner = await current.ExecuteScalarAsync(ct);
        if (owner is null || (clienteId is not null && Convert.ToInt64(owner) != clienteId)) return false;
        await using var capacity = Command(db, "SELECT capacidad FROM franjas_citas WHERE id=@p0 AND habilitada=1 FOR UPDATE", tx, cmd.FranjaId);
        var max = await capacity.ExecuteScalarAsync(ct);
        if (max is null) throw new InvalidOperationException("Franja inexistente o inhabilitada.");
        await using var occupied = Command(db,
            "SELECT COUNT(*) FROM citas WHERE franja_id=@p0 AND estado='CONFIRMADA' AND id<>@p1", tx, cmd.FranjaId, id);
        if (Convert.ToInt32(await occupied.ExecuteScalarAsync(ct)) >= Convert.ToInt32(max))
            throw new InvalidOperationException("La franja no tiene cupo.");
        await using var update = Command(db, "UPDATE citas SET franja_id=@p0,motivo=@p1 WHERE id=@p2", tx, cmd.FranjaId, cmd.Motivo.Trim(), id);
        var changed = await update.ExecuteNonQueryAsync(ct) > 0;
        await tx.CommitAsync(ct);
        return changed;
    }

    public async Task<bool> CancelarAsync(long id, long? clienteId, CancellationToken ct) =>
        await Update("UPDATE citas SET estado='CANCELADA' WHERE id=@p0 AND estado='CONFIRMADA' AND (@p1 IS NULL OR cliente_id=@p1)",
            ct, id, clienteId) > 0;

    Task<IReadOnlyList<AtencionResumen>> IServiciosStore.ListarAsync(long? clienteId, CancellationToken ct) =>
        List("SELECT id,cliente_id,motocicleta_id,estado,motivo,ingresada_en FROM atenciones WHERE (@p0 IS NULL OR cliente_id=@p0) ORDER BY ingresada_en DESC",
            r => new AtencionResumen(r.GetInt64(0), r.GetInt64(1), r.GetInt64(2), r.GetString(3), r.GetString(4), r.GetDateTime(5)), ct, clienteId);

    public Task<long> RegistrarAsync(RegistrarAtencionCommand cmd, CancellationToken ct) =>
        Insert("INSERT INTO atenciones(cliente_id,motocicleta_id,cita_id,mecanico_id,motivo,kilometraje_ingreso) VALUES(@p0,@p1,@p2,@p3,@p4,@p5)",
            ct, cmd.ClienteId, cmd.MotocicletaId, cmd.CitaId, cmd.MecanicoId, cmd.Motivo.Trim(), cmd.KilometrajeIngreso);

    public async Task<bool> ValidarAsync(long id, ValidarServicioCommand cmd, CancellationToken ct)
    {
        if (cmd.Estado is not ("DIAGNOSTICADA" or "SIN_REPARACION" or "CONTRATADA" or "CERRADA"))
            throw new ArgumentException("Estado de atención inválido.");
        return await Update("UPDATE atenciones SET estado=@p0 WHERE id=@p1 AND estado<>@p0",
            ct, cmd.Estado, id) > 0;
    }

    public Task<IReadOnlyList<GarantiaResumen>> GarantiasAsync(long? clienteId, CancellationToken ct) =>
        List("SELECT g.id,g.orden_id,g.aplica,g.inicia_en,g.vence_en,g.condiciones FROM garantias_servicio g JOIN ordenes_servicio o ON o.id=g.orden_id JOIN presupuestos p ON p.id=o.presupuesto_id JOIN atenciones a ON a.id=p.atencion_id WHERE (@p0 IS NULL OR a.cliente_id=@p0) ORDER BY g.id DESC",
            r => new GarantiaResumen(r.GetInt64(0), r.GetInt64(1), r.GetBoolean(2),
                r.IsDBNull(3) ? null : r.GetDateTime(3), r.IsDBNull(4) ? null : r.GetDateTime(4), r.GetString(5)), ct, clienteId);

    public async Task<Indicadores> ConsultarIndicadoresAsync(CancellationToken ct)
    {
        var c = Convert.ToInt32(await Scalar("SELECT COUNT(*) FROM clientes", ct));
        var m = Convert.ToInt32(await Scalar("SELECT COUNT(*) FROM motocicletas", ct));
        var a = Convert.ToInt32(await Scalar("SELECT COUNT(*) FROM citas WHERE estado='CONFIRMADA'", ct));
        var s = Convert.ToInt32(await Scalar("SELECT COUNT(*) FROM atenciones WHERE estado IN ('RECIBIDA','DIAGNOSTICADA','CONTRATADA')", ct));
        return new Indicadores(c, m, a, s);
    }

    public Task<IReadOnlyList<AvisoResumen>> ListarAsync(long clienteId, CancellationToken ct) =>
        List("SELECT id,cliente_id,tipo,mensaje,creada_en,leida_en FROM notificaciones WHERE cliente_id=@p0 ORDER BY creada_en DESC",
            r => new AvisoResumen(r.GetInt64(0), r.GetInt64(1), r.GetString(2), r.GetString(3),
                r.GetDateTime(4), r.IsDBNull(5) ? null : r.GetDateTime(5)), ct, clienteId);

    public Task<long> GenerarAsync(CrearAvisoCommand cmd, CancellationToken ct)
    {
        if (cmd.Tipo is not ("CITA_CONFIRMADA" or "AVANCE_25" or "SERVICIO_TERMINADO" or "CANCELACION_RESUELTA"))
            throw new ArgumentException("Tipo de aviso no definido en la base de datos.");
        if (cmd.ClienteId <= 0 || string.IsNullOrWhiteSpace(cmd.Mensaje) || cmd.Mensaje.Length > 400)
            throw new ArgumentException("Datos del aviso no válidos.");
        return Insert("INSERT INTO notificaciones(cliente_id,cita_id,tipo,mensaje) VALUES(@p0,@p1,@p2,@p3)",
            ct, cmd.ClienteId, cmd.CitaId, cmd.Tipo, cmd.Mensaje);
    }
}
