namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;

/// <summary>
/// Immutable configuration for a PostgreSQL container.
/// </summary>
public sealed class PostgresContainerConfig
{
    /// <summary>
    /// Gets the container key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the Docker image to use.
    /// </summary>
    public string Image { get; }

    /// <summary>
    /// Gets the database configurations.
    /// </summary>
    public IReadOnlyDictionary<string, PostgresDatabaseConfig> Databases { get; }

    /// <summary>
    /// Gets the user configurations.
    /// </summary>
    public IReadOnlyDictionary<string, PostgresUserConfig> Users { get; }

    /// <summary>
    /// Gets the memory limit (e.g., "256m", "1g").
    /// </summary>
    public string? MemoryLimit { get; }

    /// <summary>
    /// Gets the CPU limit (e.g., 0.5 for half a CPU).
    /// </summary>
    public double? CpuLimit { get; }

    internal PostgresContainerConfig(
        string key,
        string image,
        IReadOnlyDictionary<string, PostgresDatabaseConfig> databases,
        IReadOnlyDictionary<string, PostgresUserConfig> users,
        string? memoryLimit,
        double? cpuLimit)
    {
        Key = key;
        Image = image;
        Databases = databases;
        Users = users;
        MemoryLimit = memoryLimit;
        CpuLimit = cpuLimit;
    }
}
