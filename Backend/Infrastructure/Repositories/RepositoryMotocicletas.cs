namespace FollowMotors.Infrastructure.Repositories.Motocicletas;

using System.Text.Json;
using Dapper;
using FollowMotors.Application.Abstractions.Repositories.Motocicletas;
using FollowMotors.Domain.Entities.Motocicletas;
using FollowMotors.Infrastructure.Database;

/// <summary>Persiste unidades multimarca y evita consultas a propietarios ajenos.</summary>
public sealed class RepositorioMotocicletas(ConexionMySql baseDatos) : IRepositorioMotocicletas
{
    internal const string Seleccion = """
        SELECT id AS Id, cliente_id AS ClienteId, numero_serie AS NumeroSerie,
            placa AS Placa, marca AS Marca, modelo AS Modelo, anio AS Anio,
            color AS Color, kilometraje_actual AS KilometrajeActual,
            registrada_en AS RegistradaEn FROM motocicletas
        """;

    /// <summary>Registra VIN/chasis único vinculado a un cliente existente.</summary>
    public async Task<ulong> RegistrarAsync(Motocicleta motocicleta, ulong usuarioActorId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        await using var transaccion = await conexion.BeginTransactionAsync(ct);
        int filas = await conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO motocicletas (cliente_id, numero_serie, placa, marca, modelo,
                anio, color, kilometraje_actual)
            SELECT c.id, @NumeroSerie, @Placa, @Marca, @Modelo,
                @Anio, @Color, @KilometrajeActual
            FROM clientes c JOIN usuarios u ON u.id = c.usuario_id
            WHERE c.id = @ClienteId AND u.activo = 1
            """, motocicleta, transaction: transaccion, cancellationToken: ct));
        if (filas != 1) throw new KeyNotFoundException("El propietario no existe o está inactivo.");
        ulong id = await conexion.ExecuteScalarAsync<ulong>(new CommandDefinition(
            "SELECT LAST_INSERT_ID()", transaction: transaccion, cancellationToken: ct));
        await AuditoriaSql.RegistrarAsync(conexion, transaccion, usuarioActorId, "motocicletas", id,
            "ALTA_MOTOCICLETA", null,
            JsonSerializer.Serialize(new { id, motocicleta.ClienteId, motocicleta.NumeroSerie }), ct);
        await transaccion.CommitAsync(ct);
        return id;
    }

    /// <summary>Obtiene una unidad por ID para verificar su propietario.</summary>
    public async Task<Motocicleta?> ObtenerPorIdAsync(ulong id, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        return await conexion.QuerySingleOrDefaultAsync<Motocicleta>(new CommandDefinition(
            Seleccion + " WHERE id = @id", new { id }, cancellationToken: ct));
    }

    /// <summary>Lista únicamente las motocicletas asociadas al cliente indicado.</summary>
    public async Task<IReadOnlyList<Motocicleta>> ListarPorClienteAsync(ulong clienteId, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        var filas = await conexion.QueryAsync<Motocicleta>(new CommandDefinition(
            Seleccion + " WHERE cliente_id = @clienteId ORDER BY registrada_en DESC",
            new { clienteId }, cancellationToken: ct));
        return filas.AsList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Motocicleta>> BuscarAsync(string? texto, int pagina, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        string filtro = ConsultasMySql.Prefijo(texto);
        int offset = (pagina - 1) * 50;
        var filas = await db.QueryAsync<Motocicleta>(new CommandDefinition(Seleccion + """
             WHERE numero_serie LIKE @filtro ESCAPE '!' OR modelo LIKE @filtro ESCAPE '!'
                OR marca LIKE @filtro ESCAPE '!'
             ORDER BY id DESC LIMIT 50 OFFSET @offset
            """, new { filtro, offset }, cancellationToken: ct));
        return filas.AsList();
    }

    /// <inheritdoc />
    public async Task ActualizarAsync(Motocicleta moto, ulong actorId, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        var anterior = await db.QuerySingleOrDefaultAsync<Motocicleta>(new CommandDefinition(
            Seleccion + " WHERE id = @Id FOR UPDATE", moto, tx, cancellationToken: ct))
            ?? throw new KeyNotFoundException("No existe la motocicleta.");
        if (anterior.ClienteId != moto.ClienteId)
        {
            var tieneHistorial = await db.ExecuteScalarAsync<bool>(new CommandDefinition("""
                SELECT EXISTS(SELECT 1 FROM atenciones WHERE motocicleta_id = @Id)
                    OR EXISTS(SELECT 1 FROM citas WHERE motocicleta_id = @Id)
                    OR EXISTS(SELECT 1 FROM mantenimientos_programados WHERE motocicleta_id = @Id)
                """, moto, tx, cancellationToken: ct));
            if (tieneHistorial)
                throw new InvalidOperationException("El vínculo no puede cambiar después de registrar actividad.");
        }
        int filas = await db.ExecuteAsync(new CommandDefinition("""
            UPDATE motocicletas m JOIN clientes c ON c.id = @ClienteId
            JOIN usuarios u ON u.id = c.usuario_id
            SET m.cliente_id = c.id, m.numero_serie = @NumeroSerie, m.placa = @Placa,
                m.marca = @Marca, m.modelo = @Modelo, m.anio = @Anio, m.color = @Color,
                m.kilometraje_actual = @KilometrajeActual
            WHERE m.id = @Id AND (u.activo OR m.cliente_id = c.id)
            """, moto, tx, cancellationToken: ct));
        if (filas == 0) throw new KeyNotFoundException("El nuevo propietario no existe o está inactivo.");
        await AuditoriaSql.RegistrarAsync(db, tx, actorId, "motocicletas", moto.Id, "ACTUALIZAR",
            JsonSerializer.Serialize(anterior), JsonSerializer.Serialize(moto with { RegistradaEn = anterior.RegistradaEn }), ct);
        await tx.CommitAsync(ct);
    }
}
