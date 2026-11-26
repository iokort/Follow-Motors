namespace FollowMotors.Infrastructure.Repositories.Auditoria;

using Dapper;
using FollowMotors.Application.Abstractions.Repositories.Auditoria;
using FollowMotors.Domain.Entities.Auditoria;
using FollowMotors.Infrastructure.Database;

/// <summary>Proporciona lectura y registro de auditoría en MySQL.</summary>
public sealed class RepositorioAuditoria(ConexionMySql baseDatos) : IRepositorioAuditoria
{
    /// <summary>Guarda una entrada de auditoría independiente.</summary>
    public async Task RegistrarAsync(RegistroAuditoria registro, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        await conexion.ExecuteAsync(new CommandDefinition("""
            INSERT INTO auditoria (usuario_id, entidad, registro_id, accion, antes, despues)
            VALUES (@UsuarioId, @Entidad, @RegistroId, @Accion, @Antes, @Despues)
            """, registro, cancellationToken: ct));
    }

    /// <summary>Consulta las últimas entradas sin devolver la tabla completa.</summary>
    public async Task<IReadOnlyList<RegistroAuditoria>> ConsultarAsync(int limite, CancellationToken ct)
    {
        await using var conexion = await baseDatos.AbrirAsync(ct);
        var filas = await conexion.QueryAsync<RegistroAuditoria>(new CommandDefinition("""
            SELECT id AS Id, usuario_id AS UsuarioId, entidad AS Entidad, registro_id AS RegistroId,
                accion AS Accion, CAST(antes AS CHAR) AS Antes, CAST(despues AS CHAR) AS Despues,
                registrada_en AS RegistradaEn FROM auditoria ORDER BY id DESC LIMIT @limite
            """, new { limite }, cancellationToken: ct));
        return filas.AsList();
    }
}
