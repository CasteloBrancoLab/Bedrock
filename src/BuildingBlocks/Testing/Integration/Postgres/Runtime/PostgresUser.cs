using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;

namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Runtime;

/// <summary>
/// Represents a PostgreSQL user within a container.
/// </summary>
public sealed class PostgresUser
{
    private readonly PostgresUserConfig _config;

    internal PostgresUser(PostgresUserConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Gets the username.
    /// </summary>
    public string Username => _config.Username;

    /// <summary>
    /// Gets the password.
    /// </summary>
    public string Password => _config.Password;
}
