using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Models;
using Microsoft.Extensions.Configuration;
using Templates.Infra.Data.PostgreSql.Connections.Interfaces;

namespace Templates.Infra.Data.PostgreSql.Connections;

public sealed class TemplatesPostgreSqlConnection
    : PostgreSqlConnectionBase,
    ITemplatesPostgreSqlConnection
{
    // Constants
    private const string ConnectionStringConfigKey = "ConnectionStrings:TemplatesPostgreSql";

    // Fields
    private readonly IConfiguration _configuration;

    // Constructors
    public TemplatesPostgreSqlConnection(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Protected Methods
    protected override void ConfigureInternal(PostgreSqlConnectionOptions options)
    {
        string? connectionString = _configuration[ConnectionStringConfigKey];

        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString,
            nameof(connectionString)
        );

        options.WithConnectionString(connectionString);
    }
}
