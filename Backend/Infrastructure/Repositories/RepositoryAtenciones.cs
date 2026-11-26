namespace FollowMotors.Infrastructure.Repositories.Atenciones;

using System.Text.Json;
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

    private const string Seleccion = """
        SELECT id AS Id, cliente_id AS ClienteId, motocicleta_id AS MotocicletaId,
            cita_id AS CitaId, mecanico_id AS MecanicoId, motivo AS Motivo,
            kilometraje_ingreso AS KilometrajeIngreso, diagnostico AS TextoDiagnostico,
            estado AS Estado, ingresada_en AS IngresadaEn,
            diagnosticada_en AS DiagnosticadaEn FROM atenciones
        """;

    /// <summary>Crea el ingreso comprobando mecánico activo y propiedad de la motocicleta.</summary>
    public async Task<ulong> CrearAsync(AtencionTaller atencion, ulong usuarioActorId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        await using var transaccion = await conexion.BeginTransactionAsync(ct);
        int filas = await conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO atenciones (cliente_id, motocicleta_id, cita_id, mecanico_id,
                motivo, kilometraje_ingreso, estado)
            SELECT m.cliente_id, m.id, @CitaId, e.id, @Motivo, @KilometrajeIngreso, 'RECIBIDA'
            FROM motocicletas m
            JOIN clientes c ON c.id = m.cliente_id
            JOIN usuarios u ON u.id = c.usuario_id AND u.activo = 1
            JOIN empleados e ON e.id = @MecanicoId AND e.es_mecanico = 1 AND e.disponible = 1
            JOIN usuarios ue ON ue.id = e.usuario_id AND ue.activo = 1
            WHERE m.id = @MotocicletaId AND m.cliente_id = @ClienteId
              AND (@CitaId IS NULL OR EXISTS (
                   SELECT 1 FROM citas ci WHERE ci.id = @CitaId AND ci.cliente_id = @ClienteId
                   AND (ci.motocicleta_id IS NULL OR ci.motocicleta_id = @MotocicletaId)))
            """, atencion, transaction: transaccion, cancellationToken: ct));
        if (filas != 1)
            throw new InvalidOperationException("El cliente, la motocicleta, la cita o el mecánico no son válidos.");
        ulong id = await conexion.ExecuteScalarAsync<ulong>(new CommandDefinition(
            "SELECT LAST_INSERT_ID()", transaction: transaccion, cancellationToken: ct));
        await AuditoriaSql.RegistrarAsync(conexion, transaccion, usuarioActorId, "atenciones", id,
            "RECEPCION", null,
            JsonSerializer.Serialize(new { id, atencion.ClienteId, atencion.MotocicletaId,
                atencion.MecanicoId }), ct);
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
            new CommandDefinition("""
            SELECT id AS Id, importe AS Importe FROM tarifas_revision
            WHERE vigente_desde <= UTC_TIMESTAMP(3)
              AND (vigente_hasta IS NULL OR vigente_hasta > UTC_TIMESTAMP(3))
            ORDER BY vigente_desde DESC LIMIT 1
            """, transaction: transaccion, cancellationToken: ct));
        if (tarifa is null || tarifa.Id == 0) throw new InvalidOperationException("No existe tarifa de revisión vigente.");
        await conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO cobros_revision (atencion_id, tarifa_id, importe_cobrado,
                registrado_por_usuario_id, referencia_comprobante)
            VALUES (@atencionId, @tarifaId, @importe, @usuarioActorId, @referenciaComprobante)
            """, new { atencionId, tarifaId = tarifa.Id, importe = tarifa.Importe,
                usuarioActorId, referenciaComprobante }, transaction: transaccion, cancellationToken: ct));
        ulong id = await conexion.ExecuteScalarAsync<ulong>(new CommandDefinition(
            "SELECT LAST_INSERT_ID()", transaction: transaccion, cancellationToken: ct));
        await AuditoriaSql.RegistrarAsync(conexion, transaccion, usuarioActorId,
            "cobros_revision", id, "COBRO_REVISION", null,
            JsonSerializer.Serialize(new { id, atencionId, tarifa.Importe }), ct);
        await transaccion.CommitAsync(ct);
        return id;
    }
}
