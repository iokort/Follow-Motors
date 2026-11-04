using MySqlConnector;

namespace FollowMotors.Api.Infrastructure;

public interface IDatabaseConnectionFactory
{
    MySqlConnection Create();
}
