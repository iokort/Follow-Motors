namespace FollowMotors.Infrastructure.Repositories;

using Dapper;
using FollowMotors.Application.Models.Catalogos;
using FollowMotors.Application.Models.Clientes;
using FollowMotors.Application.Queries.Catalogos;
using FollowMotors.Domain.Entities.Atenciones;
using FollowMotors.Domain.Entities.Motocicletas;
using FollowMotors.Domain.Entities.Personal;
using FollowMotors.Infrastructure.Database;
using FollowMotors.Infrastructure.Repositories.Atenciones;
using FollowMotors.Infrastructure.Repositories.Clientes;
using FollowMotors.Infrastructure.Repositories.Motocicletas;
using FollowMotors.Infrastructure.Repositories.Personal;

/// <summary>Construye el catálogo con una lectura consistente de las tablas operativas.</summary>
public sealed class ConsultaCatalogoTaller(ConexionMySql baseDatos) : IConsultaCatalogoTaller
{
    /// <inheritdoc />
    public async Task<CatalogoTaller> ConsultarAsync(bool incluirConteoAuditoria, CancellationToken ct)
    {
        await using var db = await baseDatos.AbrirAsync(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        var sql = string.Join(";\n",
            RepositorioClientes.SeleccionFicha + " ORDER BY c.id DESC",
            RepositorioMotocicletas.Seleccion + " ORDER BY id DESC",
            RepositorioPersonal.Seleccion + " WHERE u.activo ORDER BY e.id",
            RepositorioAtenciones.Seleccion + " ORDER BY id DESC",
            """
            SELECT id AS Id, atencion_id AS AtencionId, importe_cobrado AS Importe,
                cobrado_en AS Fecha, referencia_comprobante AS Referencia
            FROM cobros_revision ORDER BY id DESC
            """,
            "SELECT CASE WHEN @incluirConteoAuditoria THEN (SELECT COUNT(*) FROM auditoria) ELSE 0 END",
            "SELECT (SELECT importe " + RepositorioAtenciones.TarifaVigente + ")");
        using var filas = await db.QueryMultipleAsync(new CommandDefinition(sql,
            new { incluirConteoAuditoria }, tx, cancellationToken: ct));

        var clientes = (await filas.ReadAsync<ClienteFicha>()).AsList();
        var motos = (await filas.ReadAsync<Motocicleta>()).AsList();
        var personal = (await filas.ReadAsync<Empleado>()).AsList();
        var atenciones = (await filas.ReadAsync<AtencionTaller>()).AsList();
        var cobros = (await filas.ReadAsync<CobroCatalogo>()).AsList();
        int auditorias = await filas.ReadSingleAsync<int>();
        decimal? tarifa = await filas.ReadSingleAsync<decimal?>();
        await tx.CommitAsync(ct);
        return new CatalogoTaller(clientes, motos, personal, atenciones, cobros, auditorias, tarifa);
    }
}
