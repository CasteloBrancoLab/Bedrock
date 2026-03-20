namespace Bedrock.BuildingBlocks.Resilience.Persistence.PostgreSql.Models;

/// <summary>
/// Fluent configuration for the PostgreSQL circuit breaker state store.
/// Connection is provided via <see cref="Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces.IPostgreSqlConnection"/>
/// through constructor injection — the client implementation decides connection management.
/// </summary>
public sealed class CircuitBreakerStateStoreOptions
{
    internal string Schema { get; private set; } = "bedrock";
    internal string TableName { get; private set; } = "circuit_breaker_state";

    /// <summary>
    /// Sets the database schema. Default is <c>"bedrock"</c>.
    /// </summary>
    public CircuitBreakerStateStoreOptions WithSchema(string schema)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        Schema = schema;
        return this;
    }

    /// <summary>
    /// Sets the table name. Default is <c>"circuit_breaker_state"</c>.
    /// </summary>
    public CircuitBreakerStateStoreOptions WithTableName(string tableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        TableName = tableName;
        return this;
    }
}
