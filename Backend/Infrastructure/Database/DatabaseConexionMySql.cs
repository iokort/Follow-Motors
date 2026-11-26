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
