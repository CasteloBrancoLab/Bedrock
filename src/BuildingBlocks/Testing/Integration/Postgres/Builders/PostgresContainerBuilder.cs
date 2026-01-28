using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;

namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Builders;

/// <summary>
/// Fluent builder for PostgreSQL container configuration.
/// </summary>
public sealed class PostgresContainerBuilder
{
    private readonly string _key;
    private string _image = "postgres:17";
    private readonly Dictionary<string, PostgresDatabaseBuilder> _databaseBuilders = [];
    private readonly Dictionary<string, PostgresUserBuilder> _userBuilders = [];
    private string? _memoryLimit;
    private double? _cpuLimit;

    internal PostgresContainerBuilder(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _key = key;
    }

    /// <summary>
    /// Sets the PostgreSQL Docker image.
    /// </summary>
    /// <param name="image">The Docker image (e.g., "postgres:17").</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresContainerBuilder WithImage(string image)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(image);
        _image = image;
        return this;
    }

    /// <summary>
    /// Configures a database within the container.
    /// </summary>
    /// <param name="name">The database name.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresContainerBuilder WithDatabase(
        string name,
        Action<PostgresDatabaseBuilder>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_databaseBuilders.TryGetValue(name, out var builder))
        {
            builder = new PostgresDatabaseBuilder(name);
            _databaseBuilders[name] = builder;
        }

        configure?.Invoke(builder);
        return this;
    }

    /// <summary>
    /// Configures a user within the container.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresContainerBuilder WithUser(
        string username,
        string password,
        Action<PostgresUserBuilder>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        if (!_userBuilders.TryGetValue(username, out var builder))
        {
            builder = new PostgresUserBuilder(username, password);
            _userBuilders[username] = builder;
        }

        configure?.Invoke(builder);
        return this;
    }

    /// <summary>
    /// Sets resource limits for the container.
    /// </summary>
    /// <param name="memory">Memory limit (e.g., "256m", "1g").</param>
    /// <param name="cpu">CPU limit (e.g., 0.5 for half a CPU).</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresContainerBuilder WithResourceLimits(
        string? memory = null,
        double? cpu = null)
    {
        _memoryLimit = memory;
        _cpuLimit = cpu;
        return this;
    }

    internal PostgresContainerConfig Build()
    {
        var databases = _databaseBuilders
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Build());

        var users = _userBuilders
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Build());

        return new PostgresContainerConfig(
            _key,
            _image,
            databases.AsReadOnly(),
            users.AsReadOnly(),
            _memoryLimit,
            _cpuLimit);
    }
}
