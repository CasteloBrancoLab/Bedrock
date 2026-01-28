using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Models;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Connections;

/// <summary>
/// Test PostgreSQL connection implementation for integration tests.
/// Accepts a connection string in the constructor for flexibility in test scenarios.
/// </summary>
public class TestPostgreSqlConnection : PostgreSqlConnectionBase
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestPostgreSqlConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public TestPostgreSqlConnection(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    protected override void ConfigureInternal(PostgreSqlConnectionOptions options)
    {
        options.WithConnectionString(_connectionString);
    }
}
