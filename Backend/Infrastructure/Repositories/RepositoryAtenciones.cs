namespace FollowMotors.Infrastructure.Repositories.Atenciones;

using System.Data;
using System.Text.Json;
using FollowMotors.Domain.Rules.Atenciones;
using Dapper;
using FollowMotors.Application.Abstractions.Repositories.Atenciones;
using FollowMotors.Domain.Entities.Atenciones;
using FollowMotors.Infrastructure.Database;

/// <summary>Registra ingresos, diagnósticos y cobros de tarifa fija en MySQL.</summary>
public sealed class RepositorioAtenciones(ConexionMySql baseDatos) : IRepositorioAtenciones
{
    public sealed class FilaTarifa
    {
        public ulong Id { get; set; }
        public decimal Importe { get; set; }
    }

    internal const string Seleccion = """
        SELECT id AS Id, cliente_id AS ClienteId, motocicleta_id AS MotocicletaId,
            cita_id AS CitaId, mecanico_id AS MecanicoId, motivo AS Motivo,
            kilometraje_ingreso AS KilometrajeIngreso, diagnostico AS TextoDiagnostico,
            estado AS Estado, ingresada_en AS IngresadaEn,
            diagnosticada_en AS DiagnosticadaEn FROM atenciones
        """;

    internal const string TarifaVigente = """
            FROM tarifas_revision
            WHERE vigente_desde <= UTC_TIMESTAMP(3)
              AND (vigente_hasta IS NULL OR vigente_hasta > UTC_TIMESTAMP(3))
            ORDER BY vigente_desde DESC LIMIT 1
            """;

    /// <summary>Crea el ingreso comprobando mecánico activo y propiedad de la motocicleta.</summary>
    public async Task<ulong> CrearAsync(AtencionTaller atencion, ulong usuarioActorId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        await using var bloqueo = await BloqueoMySql.AdquirirAsync(conexion, "mecanicos", ct);
        await using var transaccion = await conexion.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var candidatos = await conexion.QueryAsync<CargaMecanico>(new CommandDefinition("""
            SELECT e.id AS Id, (SELECT COUNT(*) FROM atenciones a WHERE a.mecanico_id = e.id
                AND a.estado IN ('RECIBIDA','DIAGNOSTICADA','CONTRATADA')) AS AtencionesAbiertas
            FROM empleados e JOIN usuarios u ON u.id = e.usuario_id
            WHERE e.es_mecanico AND e.disponible AND u.activo
            """, transaction: transaccion, cancellationToken: ct));
        var ingreso = atencion with { MecanicoId = AsignacionMecanicos.Elegir(candidatos) };
        if (ingreso.CitaId.HasValue)
        {
            int cita = await conexion.ExecuteAsync(new CommandDefinition("""
                UPDATE citas SET estado = 'ATENDIDA'
                WHERE id = @CitaId AND cliente_id = @ClienteId
                AND (motocicleta_id IS NULL OR motocicleta_id = @MotocicletaId) AND estado = 'CONFIRMADA'
                """, ingreso, transaccion, cancellationToken: ct));
            if (cita != 1) throw new InvalidOperationException("La cita no está confirmada para esta unidad.");
        }

        int filas = await conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO atenciones (cliente_id, motocicleta_id, cita_id, mecanico_id, motivo, kilometraje_ingreso)
            SELECT m.cliente_id, m.id, @CitaId, @MecanicoId, @Motivo, @KilometrajeIngreso
            FROM motocicletas m JOIN clientes c ON c.id = m.cliente_id
            WHERE m.id = @MotocicletaId AND c.id = @ClienteId
            """, ingreso, transaccion, cancellationToken: ct));
        if (filas != 1) throw new KeyNotFoundException("La motocicleta no pertenece al expediente indicado.");
        ulong id = await ConsultasMySql.IdNuevoAsync(conexion, transaccion, ct);
        await conexion.ExecuteAsync(new CommandDefinition("""
            UPDATE motocicletas SET kilometraje_actual = GREATEST(kilometraje_actual,@KilometrajeIngreso)
            WHERE id = @MotocicletaId
            """, ingreso, transaccion, cancellationToken: ct));
        await AuditoriaSql.RegistrarAsync(conexion, transaccion, usuarioActorId, "atenciones", id, "RECEPCION", null,
            JsonSerializer.Serialize(new { id, ingreso.ClienteId, ingreso.MotocicletaId, ingreso.MecanicoId, ingreso.CitaId }), ct);
        await transaccion.CommitAsync(ct);
        return id;
    }

    /// <summary>Consulta una atención completa por identificador.</summary>
    public async Task<AtencionTaller?> ObtenerAsync(ulong atencionId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        return await conexion.QuerySingleOrDefaultAsync<AtencionTaller>(new CommandDefinition(
            Seleccion + " WHERE id = @atencionId", new { atencionId }, cancellationToken: ct));
    }

    /// <summary>Registra hallazgos únicamente en una atención que no se ha diagnosticado.</summary>
    public async Task<bool> RegistrarDiagnosticoAsync(ulong atencionId, string hallazgos,
        ulong usuarioActorId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        await using var transaccion = await conexion.BeginTransactionAsync(ct);
        int filas = await conexion.ExecuteAsync(new CommandDefinition("""
            UPDATE atenciones SET diagnostico = @hallazgos, estado = 'DIAGNOSTICADA',
                diagnosticada_en = UTC_TIMESTAMP(3)
            WHERE id = @atencionId AND estado = 'RECIBIDA'
            """, new { atencionId, hallazgos }, transaction: transaccion, cancellationToken: ct));
        if (filas == 0) return false;
        await AuditoriaSql.RegistrarAsync(conexion, transaccion, usuarioActorId,
            "atenciones", atencionId, "DIAGNOSTICO", "{\"estado\":\"RECIBIDA\"}",
            JsonSerializer.Serialize(new { estado = "DIAGNOSTICADA", diagnostico = hallazgos }), ct);
        await transaccion.CommitAsync(ct);
        return true;
    }

    /// <summary>Cobra una sola vez el diagnóstico usando la tarifa fija activa en el momento del pago.</summary>
    public async Task<ulong> CobrarRevisionAsync(ulong atencionId, ulong usuarioActorId,
        string? referenciaComprobante, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        await using var transaccion = await conexion.BeginTransactionAsync(ct);
        string? estado = await conexion.QuerySingleOrDefaultAsync<string>(new CommandDefinition(
            "SELECT estado FROM atenciones WHERE id = @atencionId FOR UPDATE",
            new { atencionId }, transaction: transaccion, cancellationToken: ct));
        if (estado != "DIAGNOSTICADA")
            throw new InvalidOperationException("La revisión no existe o todavía no está diagnosticada.");
        var tarifa = await conexion.QuerySingleOrDefaultAsync<FilaTarifa>(
            new CommandDefinition("SELECT id AS Id, importe AS Importe " + TarifaVigente, transaction: transaccion, cancellationToken: ct));
        if (tarifa is null || tarifa.Id == 0) throw new InvalidOperationException("No existe tarifa de revisión vigente.");
        await conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO cobros_revision (atencion_id, tarifa_id, importe_cobrado,
                registrado_por_usuario_id, referencia_comprobante)
            VALUES (@atencionId, @tarifaId, @importe, @usuarioActorId, @referenciaComprobante)
            """, new { atencionId, tarifaId = tarifa.Id, importe = tarifa.Importe,
                usuarioActorId, referenciaComprobante }, transaction: transaccion, cancellationToken: ct));
        ulong id = await ConsultasMySql.IdNuevoAsync(conexion, transaccion, ct);
        await AuditoriaSql.RegistrarAsync(conexion, transaccion, usuarioActorId,
            "cobros_revision", id, "COBRO_REVISION", null,
            JsonSerializer.Serialize(new { id, atencionId, tarifa.Importe }), ct);
        await transaccion.CommitAsync(ct);
        return id;
    }

    /// <inheritdoc />
    public async Task CerrarSinReparacionAsync(ulong atencionId, ulong usuarioActorId, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        int modificadas = await db.ExecuteAsync(new CommandDefinition("""
            UPDATE atenciones a SET a.estado = 'SIN_REPARACION' WHERE a.id = @atencionId AND a.estado = 'DIAGNOSTICADA'
                AND EXISTS(SELECT 1 FROM cobros_revision c WHERE c.atencion_id = a.id)
                AND NOT EXISTS(SELECT 1 FROM presupuestos p WHERE p.atencion_id = a.id)
            """, new { atencionId }, tx, cancellationToken: ct));
        if (modificadas != 1) throw new InvalidOperationException("Se requiere una revisión cobrada sin presupuesto.");
        await AuditoriaSql.RegistrarAsync(db, tx, usuarioActorId, "atenciones", atencionId, "CERRAR_REVISION",
            null, null, ct);
        await tx.CommitAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ulong> ConfigurarTarifaAsync(decimal importe, ulong actorId, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var bloqueo = await BloqueoMySql.AdquirirAsync(db, "tarifa_revision", ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        var anterior = await db.QuerySingleOrDefaultAsync<FilaTarifa>(new CommandDefinition(
            "SELECT id AS Id, importe AS Importe " + TarifaVigente, transaction: tx, cancellationToken: ct));
        if (anterior?.Importe == importe) return anterior.Id;
        var ahora = await db.ExecuteScalarAsync<DateTime>(new CommandDefinition(
            "SELECT UTC_TIMESTAMP(3)", transaction: tx, cancellationToken: ct));
        if (anterior is not null)
            await db.ExecuteAsync(new CommandDefinition("""
                UPDATE tarifas_revision SET vigente_hasta = @ahora WHERE id = @Id
                """, new { anterior.Id, ahora }, tx, cancellationToken: ct));
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO tarifas_revision (importe,vigente_desde,vigente_hasta)
            SELECT @importe,@ahora,MIN(vigente_desde) FROM tarifas_revision WHERE vigente_desde > @ahora
            """, new { importe, ahora }, tx, cancellationToken: ct));
        ulong id = await ConsultasMySql.IdNuevoAsync(db, tx, ct);
        await AuditoriaSql.RegistrarAsync(db, tx, actorId, "tarifas_revision", id, "CONFIGURAR",
            anterior is null ? null : JsonSerializer.Serialize(anterior), JsonSerializer.Serialize(new { importe }), ct);
        await tx.CommitAsync(ct);
        return id;
    }
}
