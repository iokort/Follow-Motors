namespace FollowMotors.Infrastructure.Database;

using MySqlConnector;

/// <summary>Fábrica de conexiones independientes a la base MySQL.</summary>
public sealed class ConexionMySql(string cadenaConexion)
{
    /// <summary>Abre una conexión para una operación corta de un repositorio.</summary>
    public async Task<MySqlConnection> AbrirAsync(CancellationToken ct)
    {
        var conexion = new MySqlConnection(cadenaConexion);
        await conexion.OpenAsync(ct);
        return conexion;
    }
}

/// <summary>Ayuda a construir parámetros de búsqueda sin interpretar comodines de entrada.</summary>
internal static class ConsultasMySql
{
    /// <summary>Escapa el prefijo para un LIKE parametrizado que usa el carácter !.</summary>
    internal static string Prefijo(string? texto) =>
        (texto ?? "").Replace("!", "!!").Replace("%", "!%").Replace("_", "!_") + "%";
}
