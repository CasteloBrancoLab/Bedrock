namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;

/// <summary>
/// Immutable configuration for a PostgreSQL database within a container.
/// </summary>
public sealed class PostgresDatabaseConfig
{
    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the list of seed script file paths to execute during initialization.
    /// </summary>
    public IReadOnlyList<string> SeedScriptPaths { get; }

    /// <summary>
    /// Gets the list of inline SQL statements to execute during initialization.
    /// </summary>
    public IReadOnlyList<string> SeedSqlStatements { get; }

    internal PostgresDatabaseConfig(
        string name,
        IReadOnlyList<string> seedScriptPaths,
        IReadOnlyList<string> seedSqlStatements)
    {
        Name = name;
        SeedScriptPaths = seedScriptPaths;
        SeedSqlStatements = seedSqlStatements;
    }
}
