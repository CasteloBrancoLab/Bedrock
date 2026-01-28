using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;

namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Runtime;

/// <summary>
/// Represents a running PostgreSQL database within a container.
/// </summary>
public sealed class PostgresDatabase
{
    private readonly PostgresDatabaseConfig _config;
    private readonly string _host;
    private readonly int _port;
    private readonly string _adminPassword;

    internal PostgresDatabase(
        PostgresDatabaseConfig config,
        string host,
        int port,
        string adminPassword)
    {
        _config = config;
        _host = host;
        _port = port;
        _adminPassword = adminPassword;
    }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string Name => _config.Name;

    /// <summary>
    /// Gets the connection string for the specified user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <returns>The connection string.</returns>
    public string GetConnectionString(string username, string password)
    {
        return $"Host={_host};Port={_port};Database={_config.Name};" +
               $"Username={username};Password={password};" +
               "SSL Mode=Disable;Include Error Detail=true";
    }

    /// <summary>
    /// Gets the admin connection string (postgres user).
    /// </summary>
    internal string GetAdminConnectionString()
    {
        return $"Host={_host};Port={_port};Database={_config.Name};" +
               $"Username=postgres;Password={_adminPassword};" +
               "SSL Mode=Disable;Include Error Detail=true";
    }
}
