using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Models;
using ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces;

namespace ShopDemo.IntegrationTests.Auth.Domain.Connections;

/// <summary>
/// Test connection implementation for Auth Domain integration tests.
/// Accepts a connection string directly in the constructor.
/// </summary>
public class TestAuthPostgreSqlConnection : PostgreSqlConnectionBase, IAuthPostgreSqlConnection
{
    private readonly string _connectionString;

    public TestAuthPostgreSqlConnection(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    protected override void ConfigureInternal(PostgreSqlConnectionOptions options)
    {
        options.WithConnectionString(_connectionString);
    }
}
