namespace FollowMotors.Infrastructure.Repositories;

using System.Data;
using System.Text.Json;
using Dapper;
using FollowMotors.Application.Agenda;
using FollowMotors.Application.Operaciones;
using FollowMotors.Domain.Rules.Agenda;
using FollowMotors.Infrastructure.Database;
using MySqlConnector;

/// <summary>Persiste franjas y citas con bloqueo antes de comprobar ocupación y titularidad.</summary>
public sealed class RepositorioAgenda(ConexionMySql baseDatos) : IRepositorioAgenda
{
    private const string SeleccionFranja = """
        SELECT f.id AS Id, f.inicio AS Inicio, f.fin AS Fin, f.capacidad AS Capacidad,
            f.habilitada AS Habilitada, (SELECT COUNT(*) FROM citas c
                WHERE c.franja_id = f.id AND c.estado = 'CONFIRMADA') AS ReservasConfirmadas
        FROM franjas_citas f
        """;

    private const string SeleccionCita = """
        SELECT c.id AS Id, c.franja_id AS FranjaId, c.cliente_id AS ClienteId,
            c.motocicleta_id AS MotocicletaId, c.motivo AS Motivo, c.estado AS Estado,
            c.origen AS Origen, f.inicio AS Inicio, f.fin AS Fin
        FROM citas c JOIN franjas_citas f ON f.id = c.franja_id
        """;

    private const string PropiedadCita = """
        (@Rol IN ('GERENTE','EMPLEADO') OR EXISTS (SELECT 1 FROM clientes t
            WHERE t.id = c.cliente_id AND t.usuario_id = @UsuarioId))
        """;

    /// <inheritdoc />
    public async Task<ulong> GuardarFranjaAsync(GuardarFranjaCommand solicitud, ulong actorId, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var bloqueo = await BloqueoMySql.AdquirirAsync(db, "franjas_citas", ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var (inicio, fin, capacidad, habilitada) = solicitud.Datos;
        var datos = new { solicitud.Id, Inicio = inicio.UtcDateTime, Fin = fin.UtcDateTime,
            capacidad, habilitada, actorId };
        FranjaAgenda? anterior = solicitud.Id is ulong id ? await BloquearFranja(db, tx, id, ct) : null;
        if (anterior is not null)
            ControlCapacidad.ValidarCambio(capacidad, anterior.ReservasConfirmadas,
                habilitada && anterior.Inicio == datos.Inicio && anterior.Fin == datos.Fin);
        if (habilitada && await db.ExecuteScalarAsync<bool>(new CommandDefinition("""
                SELECT EXISTS(SELECT 1 FROM franjas_citas
                    WHERE habilitada AND inicio < @Fin AND fin > @Inicio AND (@Id IS NULL OR id <> @Id))
                """, datos, tx, cancellationToken: ct)))
            throw new InvalidOperationException("El horario se cruza con una franja habilitada.");

        string sql = anterior is null
            ? """
                INSERT INTO franjas_citas (inicio,fin,capacidad,habilitada,creada_por_usuario_id)
                VALUES (@Inicio,@Fin,@capacidad,@habilitada,@actorId)
                """
            : "UPDATE franjas_citas SET inicio=@Inicio,fin=@Fin,capacidad=@capacidad,habilitada=@habilitada WHERE id=@Id";
        await db.ExecuteAsync(new CommandDefinition(sql, datos, tx, cancellationToken: ct));
        ulong franjaId = anterior?.Id ?? await ConsultasMySql.IdNuevoAsync(db, tx, ct);
        await AuditoriaSql.RegistrarAsync(db, tx, actorId, "franjas_citas", franjaId,
            anterior is null ? "CREAR" : "EDITAR", anterior is null ? null : JsonSerializer.Serialize(anterior),
            JsonSerializer.Serialize(solicitud.Datos), ct);
        await tx.CommitAsync(ct);
        return franjaId;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FranjaAgenda>> FranjasAsync(FranjasQuery consulta, DateTime ahora, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        var filas = await db.QueryAsync<FranjaAgenda>(new CommandDefinition(
            "SELECT * FROM (" + SeleccionFranja + " WHERE f.inicio >= @desde AND f.inicio < @hasta) f " +
            "WHERE NOT @Disponibles OR (Habilitada AND Inicio > @ahora AND ReservasConfirmadas < Capacidad) " +
            "ORDER BY Inicio", new { desde = consulta.Desde.UtcDateTime, hasta = consulta.Hasta.UtcDateTime,
                consulta.Disponibles, ahora }, cancellationToken: ct));
        return filas.AsList();
    }

    /// <inheritdoc />
    public async Task<ulong> RegistrarAsync(RegistrarCitaCommand solicitud, ulong actorId, DateTime ahora, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var franja = await BloquearFranja(db, tx, solicitud.FranjaId, ct);
        await ExigirEspacio(db, tx, franja, solicitud.MotocicletaId, ahora, ct);
        var motoId = await db.QuerySingleOrDefaultAsync<ulong?>(new CommandDefinition("""
            SELECT m.id FROM motocicletas m JOIN clientes c ON c.id = m.cliente_id
            JOIN usuarios u ON u.id = c.usuario_id
            WHERE m.id = @MotocicletaId AND c.id = @ClienteId AND u.activo FOR UPDATE
            """, solicitud, tx, cancellationToken: ct));
        if (motoId is null) throw new KeyNotFoundException("La unidad no pertenece a un expediente activo.");
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO citas (franja_id,cliente_id,motocicleta_id,motivo,origen)
            VALUES (@FranjaId,@ClienteId,@MotocicletaId,@motivo,'PRESENCIAL')
            """, new { solicitud.FranjaId, solicitud.ClienteId, solicitud.MotocicletaId,
                motivo = solicitud.Motivo.Trim() }, tx, cancellationToken: ct));
        var citaId = await ConsultasMySql.IdNuevoAsync(db, tx, ct);
        await ConfirmarCita(db, tx, citaId, solicitud.ClienteId, franja, ct);
        await AuditoriaSql.RegistrarAsync(db, tx, actorId, "citas", citaId, "CONFIRMAR", null,
            JsonSerializer.Serialize(new { solicitud.FranjaId, solicitud.MotocicletaId }), ct);
        await tx.CommitAsync(ct);
        return citaId;
    }

    /// <inheritdoc />
    public async Task ReprogramarAsync(ReprogramarCitaCommand solicitud, ulong actorId, DateTime ahora, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var cita = await BloquearCita(db, tx, solicitud.Id, ct);
        if (cita.Estado != "CONFIRMADA") throw new InvalidOperationException("La cita ya fue cerrada.");
        if (cita.FranjaId == solicitud.FranjaId) return;

        var franja = await BloquearFranja(db, tx, solicitud.FranjaId, ct);
        await ExigirEspacio(db, tx, franja, cita.MotocicletaId, ahora, ct);
        await db.ExecuteAsync(new CommandDefinition("UPDATE citas SET franja_id = @FranjaId WHERE id = @Id",
            solicitud, tx, cancellationToken: ct));
        await ConfirmarCita(db, tx, cita.Id, cita.ClienteId, franja, ct);
        await AuditoriaSql.RegistrarAsync(db, tx, actorId, "citas", cita.Id, "REPROGRAMAR",
            JsonSerializer.Serialize(new { cita.FranjaId }), JsonSerializer.Serialize(new { solicitud.FranjaId }), ct);
        await tx.CommitAsync(ct);
    }

    /// <inheritdoc />
    public async Task CerrarAsync(CerrarCitaCommand solicitud, ulong actorId, DateTime ahora, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var cita = await BloquearCita(db, tx, solicitud.Id, ct);
        string estado = solicitud.NoAsistio ? "NO_ASISTIO" : "CANCELADA";
        if (cita.Estado == estado) return;
        if (cita.Estado != "CONFIRMADA" || (solicitud.NoAsistio && cita.Fin > ahora))
            throw new InvalidOperationException("El estado o el horario impiden cerrar esta cita.");
        await db.ExecuteAsync(new CommandDefinition("UPDATE citas SET estado = @estado WHERE id = @Id",
            new { estado, solicitud.Id }, tx, cancellationToken: ct));
        await AuditoriaSql.RegistrarAsync(db, tx, actorId, "citas", cita.Id, estado,
            JsonSerializer.Serialize(new { cita.Estado }), JsonSerializer.Serialize(new { estado }), ct);
        await tx.CommitAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CitaAgenda>> CitasAsync(CitasQuery consulta, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        var filas = await db.QueryAsync<CitaAgenda>(new CommandDefinition(SeleccionCita + " WHERE " + PropiedadCita + """
             AND (@desde IS NULL OR f.inicio >= @desde) AND (@hasta IS NULL OR f.inicio < @hasta)
             ORDER BY f.inicio DESC, c.id DESC LIMIT 50 OFFSET @offset
            """, new { desde = consulta.Desde?.UtcDateTime, hasta = consulta.Hasta?.UtcDateTime,
                actor.Rol, actor.UsuarioId, offset = (consulta.Pagina - 1) * 50 }, cancellationToken: ct));
        return filas.AsList();
    }

    /// <inheritdoc />
    public async Task<CitaAgenda?> CitaAsync(ulong id, ActorTaller actor, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        return await db.QuerySingleOrDefaultAsync<CitaAgenda>(new CommandDefinition(
            SeleccionCita + " WHERE c.id = @id AND " + PropiedadCita, new { id, actor.Rol, actor.UsuarioId },
            cancellationToken: ct));
    }

    /// <summary>Bloquea la fila de la franja antes de contar nuevas reservas.</summary>
    private static async Task<FranjaAgenda> BloquearFranja(MySqlConnection db, MySqlTransaction tx, ulong id, CancellationToken ct)
    {
        var franja = await db.QuerySingleOrDefaultAsync<FranjaAgenda>(new CommandDefinition(
            SeleccionFranja + " WHERE f.id = @id FOR UPDATE", new { id }, tx, cancellationToken: ct))
            ?? throw new KeyNotFoundException("No existe la franja.");
        long reservas = await db.ExecuteScalarAsync<long>(new CommandDefinition("""
            SELECT COUNT(*) FROM citas WHERE franja_id = @id AND estado = 'CONFIRMADA'
            """, new { id }, tx, cancellationToken: ct));
        return franja with { ReservasConfirmadas = reservas };
    }

    /// <summary>Bloquea la cita antes de decidir si su estado admite cambios concurrentes.</summary>
    private static async Task<CitaAgenda> BloquearCita(MySqlConnection db, MySqlTransaction tx, ulong id, CancellationToken ct) =>
        await db.QuerySingleOrDefaultAsync<CitaAgenda>(new CommandDefinition(
            SeleccionCita + " WHERE c.id = @id FOR UPDATE OF c", new { id }, tx, cancellationToken: ct))
        ?? throw new KeyNotFoundException("No existe la cita.");

    /// <summary>Comprueba el cupo y evita dos citas confirmadas para la misma moto en la franja.</summary>
    private static async Task ExigirEspacio(MySqlConnection db, MySqlTransaction tx, FranjaAgenda franja,
        ulong? motoId, DateTime ahora, CancellationToken ct)
    {
        if (!franja.Habilitada || franja.Inicio <= ahora
            || !ControlCapacidad.HayDisponibilidad(franja.Capacidad, franja.ReservasConfirmadas))
            throw new InvalidOperationException("La franja está cerrada, vencida o sin cupos.");
        if (motoId.HasValue && await db.ExecuteScalarAsync<bool>(new CommandDefinition("""
                SELECT EXISTS(SELECT 1 FROM citas WHERE franja_id = @Id AND motocicleta_id = @motoId
                    AND estado = 'CONFIRMADA')
                """, new { franja.Id, motoId }, tx, cancellationToken: ct)))
            throw new InvalidOperationException("La unidad ya tiene una cita en ese horario.");
    }

    /// <summary>Guarda la confirmación de una cita nueva o reprogramada en su bandeja interna.</summary>
    private static Task ConfirmarCita(MySqlConnection db, MySqlTransaction tx, ulong citaId, ulong clienteId,
        FranjaAgenda franja, CancellationToken ct) =>
        db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO notificaciones (cliente_id,cita_id,tipo,mensaje)
            VALUES (@clienteId,@citaId,'CITA_CONFIRMADA',@mensaje)
            """, new { clienteId, citaId, mensaje = $"Tu cita fue confirmada para {franja.Inicio:u}." },
            tx, cancellationToken: ct));
}
