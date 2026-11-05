using System.Text.Json;
using FollowMotors.Api.Application.Agenda;
using FollowMotors.Api.Domain.Agenda;
using MySqlConnector;

namespace FollowMotors.Api.Infrastructure.Agenda;

/// <summary>
/// Implementación MySQL del puerto IAgendaRepository (D de SOLID).
/// Ejecuta SQL parametrizado sobre el esquema V3 de Gael sin alterar sus tablas.
/// Regla técnica: escritura de citas y bitácora dentro de la MISMA transacción.
/// </summary>
public sealed class MySqlAgendaRepository(IDatabaseConnectionFactory factory) : IAgendaRepository
{
    // Seleccionar expresamente los campos evita depender del orden físico de las columnas.
    private const string CitaColumns = """
        id, tipo_motocicleta, motocicleta_id, marca_externa, modelo_externo,
        contacto_externo, telefono_externo, inicio_local, duracion_minutos,
        motivo, estado, registrada_por
        """;

    /// <summary>US15: recuperar cita por su ID; no usar consultas concatenadas.</summary>
    public async Task<Cita?> BuscarAsync(Guid id, CancellationToken ct)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync(ct);
        await using var command = new MySqlCommand(
            $"SELECT {CitaColumns} FROM citas WHERE id = @Id LIMIT 1", connection);
        command.Parameters.AddWithValue("@Id", id.ToString());

        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? LeerCita(reader) : null;
    }

    /// <summary>
    /// US15: consulta diaria ordenada e indexable, con máximo 200 resultados
    /// para evitar que una petición descargue innecesariamente toda la tabla.
    /// </summary>
    public async Task<IReadOnlyList<Cita>> ListarDiaAsync(DateOnly dia, CancellationToken ct)
    {
        var inicio = dia.ToDateTime(TimeOnly.MinValue);
        var fin = inicio.AddDays(1);

        await using var connection = factory.Create();
        await connection.OpenAsync(ct);
        await using var command = new MySqlCommand(
            $"SELECT {CitaColumns} FROM citas WHERE inicio_local >= @Inicio AND inicio_local < @Fin ORDER BY inicio_local, id LIMIT 200",
            connection);
        command.Parameters.AddWithValue("@Inicio", inicio);
        command.Parameters.AddWithValue("@Fin", fin);

        var resultado = new List<Cita>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            resultado.Add(LeerCita(reader));

        return resultado;
    }

    /// <summary>
    /// US06: inserta cita y auditoría de la operación de forma atómica.
    /// Una moto externa nunca se inserta en "motocicletas".
    /// </summary>
    public async Task RegistrarAsync(Cita cita, CancellationToken ct)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);

        // Mensaje funcional para una moto del portal que no esté registrada.
        // La FK de MySQL sigue siendo la defensa final ante errores de concurrencia.
        if (cita.TipoMotocicleta == "CFMOTO")
        {
            await using var check = new MySqlCommand(
                "SELECT COUNT(*) FROM motocicletas WHERE id = @Moto AND estado = 'ACTIVA'",
                connection, tx);
            check.Parameters.AddWithValue("@Moto", cita.MotocicletaId!.Value.ToString());
            var existe = Convert.ToInt64(await check.ExecuteScalarAsync(ct));
            if (existe == 0)
                throw new AgendaException("La motocicleta CFMOTO no está registrada o está inactiva.");
        }

        await using (var insert = new MySqlCommand("""
            INSERT INTO citas
            (id, tipo_motocicleta, motocicleta_id, marca_externa, modelo_externo,
             contacto_externo, telefono_externo, inicio_local, zona_horaria,
             duracion_minutos, motivo, estado, registrada_por)
            VALUES
            (@Id, @Tipo, @Moto, @Marca, @Modelo, @Contacto, @Telefono,
             @Inicio, 'America/Mexico_City', @Duracion, @Motivo, 'PROGRAMADA', @Actor)
            """, connection, tx))
        {
            insert.Parameters.AddWithValue("@Id", cita.Id.ToString());
            insert.Parameters.AddWithValue("@Tipo", cita.TipoMotocicleta);
            insert.Parameters.AddWithValue("@Moto", Db(cita.MotocicletaId?.ToString()));
            insert.Parameters.AddWithValue("@Marca", Db(cita.MarcaExterna));
            insert.Parameters.AddWithValue("@Modelo", Db(cita.ModeloExterno));
            insert.Parameters.AddWithValue("@Contacto", Db(cita.ContactoExterno));
            insert.Parameters.AddWithValue("@Telefono", Db(cita.TelefonoExterno));
            insert.Parameters.AddWithValue("@Inicio", cita.InicioLocal);
            insert.Parameters.AddWithValue("@Duracion", cita.DuracionMinutos);
            insert.Parameters.AddWithValue("@Motivo", cita.Motivo);
            insert.Parameters.AddWithValue("@Actor", cita.RegistradaPor.ToString());
            await insert.ExecuteNonQueryAsync(ct);
        }

        await AuditarAsync(connection, tx, cita.Id, cita.RegistradaPor,
            "CITA_CREADA", new { cita.TipoMotocicleta, cita.MotocicletaId, cita.InicioLocal }, ct);
        await tx.CommitAsync(ct);
    }

    /// <summary>
    /// US15: comparación optimista de estado y fecha para no perder cambios
    /// concurrentes; 0 filas actualizadas = devolver 409 desde el servicio.
    /// </summary>
    public async Task<bool> ReprogramarAsync(
        Cita anterior, DateTime inicio, int duracion, Guid actor, CancellationToken ct)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        await using var update = new MySqlCommand("""
            UPDATE citas
            SET inicio_local = @Inicio, duracion_minutos = @Duracion
            WHERE id = @Id
              AND estado = @EstadoAnterior
              AND inicio_local = @FechaAnterior
              AND duracion_minutos = @DuracionAnterior
              AND estado IN ('PROGRAMADA', 'CONFIRMADA')
            """, connection, tx);
        update.Parameters.AddWithValue("@Inicio", inicio);
        update.Parameters.AddWithValue("@Duracion", duracion);
        update.Parameters.AddWithValue("@Id", anterior.Id.ToString());
        update.Parameters.AddWithValue("@EstadoAnterior", anterior.Estado);
        update.Parameters.AddWithValue("@FechaAnterior", anterior.InicioLocal);
        update.Parameters.AddWithValue("@DuracionAnterior", anterior.DuracionMinutos);

        if (await update.ExecuteNonQueryAsync(ct) != 1)
            return false; // Rollback automático al desechar la transacción.

        await AuditarAsync(connection, tx, anterior.Id, actor,
            "CITA_REPROGRAMADA",
            new { inicioAnterior = anterior.InicioLocal, inicioNuevo = inicio,
                  duracionAnterior = anterior.DuracionMinutos, duracionNueva = duracion }, ct);
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>
    /// US15: cancelar es cambiar el estado, no borrar una fila. Mantener
    /// motivo, quién lo decidió y cuándo mediante auditoría transaccional.
    /// </summary>
    public async Task<bool> CancelarAsync(Cita anterior, string motivo, Guid actor, CancellationToken ct)
    {
        await using var connection = factory.Create();
        await connection.OpenAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        await using var update = new MySqlCommand("""
            UPDATE citas
            SET estado = 'CANCELADA'
            WHERE id = @Id AND estado = @EstadoAnterior
              AND inicio_local = @FechaAnterior
              AND estado IN ('PROGRAMADA', 'CONFIRMADA')
            """, connection, tx);
        update.Parameters.AddWithValue("@Id", anterior.Id.ToString());
        update.Parameters.AddWithValue("@EstadoAnterior", anterior.Estado);
        update.Parameters.AddWithValue("@FechaAnterior", anterior.InicioLocal);

        if (await update.ExecuteNonQueryAsync(ct) != 1)
            return false;

        await AuditarAsync(connection, tx, anterior.Id, actor,
            "CITA_CANCELADA", new { motivo, estadoAnterior = anterior.Estado }, ct);
        await tx.CommitAsync(ct);
        return true;
    }

    /// <summary>
    /// Corresponde exactamente a la tabla "auditoria" de SQL 04 de Gael.
    /// Nunca guardar contraseñas, tokens ni datos personales en el detalle.
    /// </summary>
    private static async Task AuditarAsync(
        MySqlConnection connection, MySqlTransaction tx, Guid citaId,
        Guid actor, string accion, object detalle, CancellationToken ct)
    {
        await using var command = new MySqlCommand("""
            INSERT INTO auditoria
            (id, actor_tipo, actor_id, accion, entidad, entidad_id, detalle, fecha_utc)
            VALUES (@Id, 'USUARIO', @Actor, @Accion, 'citas', @Cita,
                    @Detalle, @Fecha)
            """, connection, tx);
        command.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
        command.Parameters.AddWithValue("@Actor", actor.ToString());
        command.Parameters.AddWithValue("@Accion", accion);
        command.Parameters.AddWithValue("@Cita", citaId.ToString());
        command.Parameters.AddWithValue("@Detalle", JsonSerializer.Serialize(detalle));
        command.Parameters.AddWithValue("@Fecha", DateTime.UtcNow);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static object Db(string? value) => value is null ? DBNull.Value : value;

    /// <summary>Traducir filas SQL a modelo de dominio sin exponer conexiones.</summary>
    private static Cita LeerCita(MySqlDataReader row)
    {
        string? Optional(string name)
        {
            var index = row.GetOrdinal(name);
            return row.IsDBNull(index) ? null : row.GetString(index);
        }

        // MySqlConnector convierte CHAR(36) a Guid automáticamente por defecto.
        // Usar GetGuid evita InvalidCastException: Guid no es String.
        var motoIndex = row.GetOrdinal("motocicleta_id");
        Guid? moto = row.IsDBNull(motoIndex) ? null : row.GetGuid(motoIndex);
        return new Cita(
            row.GetGuid("id"),
            row.GetString("tipo_motocicleta"),
            moto,
            Optional("marca_externa"),
            Optional("modelo_externo"),
            Optional("contacto_externo"),
            Optional("telefono_externo"),
            row.GetDateTime("inicio_local"),
            row.GetInt32("duracion_minutos"),
            row.GetString("motivo"),
            row.GetString("estado"),
            row.GetGuid("registrada_por"));
    }
}
