namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Models;

public class PostgreSqlConnectionOptions
{
    // Properties
    public string? ConnectionString { get; private set; }


    // Public Methods
    public PostgreSqlConnectionOptions WithConnectionString(string connectionString)
    {
        ConnectionString = connectionString;

        return this;
    }
}
