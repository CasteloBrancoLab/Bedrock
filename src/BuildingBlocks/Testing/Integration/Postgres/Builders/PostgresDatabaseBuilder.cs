using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;

namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Builders;

/// <summary>
/// Fluent builder for PostgreSQL database configuration.
/// </summary>
public sealed class PostgresDatabaseBuilder
{
    private readonly string _name;
    private readonly List<string> _seedScriptPaths = [];
    private readonly List<string> _seedSqlStatements = [];

    internal PostgresDatabaseBuilder(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _name = name;
    }

    /// <summary>
    /// Adds a SQL script file to be executed during database initialization.
    /// </summary>
    /// <param name="scriptPath">The path to the SQL script file.</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresDatabaseBuilder WithSeedScript(string scriptPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptPath);
        _seedScriptPaths.Add(scriptPath);
        return this;
    }

    /// <summary>
    /// Adds a SQL statement to be executed during database initialization.
    /// </summary>
    /// <param name="sql">The SQL statement to execute.</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresDatabaseBuilder WithSeedSql(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        _seedSqlStatements.Add(sql);
        return this;
    }

    internal PostgresDatabaseConfig Build()
    {
        return new PostgresDatabaseConfig(
            _name,
            _seedScriptPaths.AsReadOnly(),
            _seedSqlStatements.AsReadOnly());
    }
}
