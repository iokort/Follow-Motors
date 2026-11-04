using MySqlConnector;

namespace FollowMotors.Api.Infrastructure;

public sealed class MySqlConnectionFactory(IConfiguration configuration) : IDatabaseConnectionFactory
{
    public MySqlConnection Create()
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Falta configurar ConnectionStrings:Default.");
        }

        return new MySqlConnection(connectionString);
    }
}
