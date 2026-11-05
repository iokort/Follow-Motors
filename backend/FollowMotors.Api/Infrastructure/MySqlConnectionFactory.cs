// PSEUDOCÓDIGO | INICIO: importar dependencias para la conexión y autenticación del sistema.
using MySqlConnector;

// PSEUDOCÓDIGO | INICIO: ubicar esta clase en el módulo de infraestructura.
namespace FollowMotors.Api.Infrastructure;

// PSEUDOCÓDIGO | INICIO CLASE: generar conexiones usando la configuración externa de MySQL.
public sealed class MySqlConnectionFactory(IConfiguration configuration) : IDatabaseConnectionFactory
// PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones del conjunto.
{
    // PSEUDOCÓDIGO | PROCESO: ejecutar la instrucción correspondiente a este paso del flujo.
    public MySqlConnection Create()
    // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones del conjunto.
    {
        // PSEUDOCÓDIGO | PROCESO: ejecutar la instrucción correspondiente a este paso del flujo.
        var connectionString = configuration.GetConnectionString("Default");
        // PSEUDOCÓDIGO | PROCESO: ejecutar la instrucción correspondiente a este paso del flujo.
        if (string.IsNullOrWhiteSpace(connectionString))
        // PSEUDOCÓDIGO | INICIO BLOQUE: ejecutar las instrucciones del conjunto.
        {
            // PSEUDOCÓDIGO | DATO: leer configuración de conexión sin escribir contraseñas en el código.
            throw new InvalidOperationException("Falta configurar ConnectionStrings:Default.");
        // PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura.
        }

        // PSEUDOCÓDIGO | DATOS: crear una conexión al servidor configurado.
        return new MySqlConnection(connectionString);
    // PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura.
    }
// PSEUDOCÓDIGO | FIN BLOQUE: terminar la estructura.
}
