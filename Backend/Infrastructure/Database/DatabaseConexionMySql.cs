namespace FollowMotors.Infrastructure.Database;

using MySqlConnector;
using Dapper;

/// <summary>Fábrica de conexiones independientes a la base MySQL.</summary>
public sealed class ConexionMySql(string cadenaConexion)
{
    /// <summary>Abre una conexión para una operación corta de un repositorio.</summary>
    public async Task<MySqlConnection> AbrirAsync(CancellationToken ct)
    {
        var cadena = new MySqlConnectionStringBuilder(cadenaConexion) { DateTimeKind = MySqlDateTimeKind.Utc };
        var conexion = new MySqlConnection(cadena.ConnectionString);
        try
        {
            await conexion.OpenAsync(ct);
            await conexion.ExecuteAsync(new CommandDefinition("SET time_zone = '+00:00'", cancellationToken: ct));
            return conexion;
        }
        catch
        {
            await conexion.DisposeAsync();
            throw;
        }
    }
}

/// <summary>Ayuda a construir parámetros de búsqueda sin interpretar comodines de entrada.</summary>
internal static class ConsultasMySql
{
    /// <summary>Obtiene el ID del INSERT en la misma conexión y transacción.</summary>
    internal static Task<ulong> IdNuevoAsync(MySqlConnection conexion, MySqlTransaction tx, CancellationToken ct) =>
        conexion.ExecuteScalarAsync<ulong>(new CommandDefinition("SELECT LAST_INSERT_ID()", transaction: tx, cancellationToken: ct));


    /// <summary>Escapa el prefijo para un LIKE parametrizado que usa el carácter !.</summary>
    internal static string Prefijo(string? texto) =>
        (texto ?? "").Replace("!", "!!").Replace("%", "!%").Replace("_", "!_") + "%";
}

/// <summary>Serializa operaciones que deben comprobar un conjunto de filas antes de insertar.</summary>
internal sealed class BloqueoMySql(MySqlConnection conexion, string recurso) : IAsyncDisposable
{
    /// <summary>Adquiere un bloqueo exclusivo por esquema y recurso compartido.</summary>
    internal static async Task<BloqueoMySql> AdquirirAsync(MySqlConnection conexion, string recurso, CancellationToken ct)
    {
        int? adquirido = await conexion.ExecuteScalarAsync<int?>(new CommandDefinition(
            "SELECT GET_LOCK(SHA2(CONCAT(DATABASE(), ':', @recurso), 256), 5)",
            new { recurso }, cancellationToken: ct));
        if (adquirido != 1) throw new InvalidOperationException("La operación está ocupada. Inténtalo nuevamente.");
        return new BloqueoMySql(conexion, recurso);
    }

    /// <summary>Libera el bloqueo incluso si la solicitud original fue cancelada.</summary>
    public async ValueTask DisposeAsync() =>
        await conexion.ExecuteAsync(new CommandDefinition(
            "SELECT RELEASE_LOCK(SHA2(CONCAT(DATABASE(), ':', @recurso), 256))",
            new { recurso }, cancellationToken: CancellationToken.None));
}
