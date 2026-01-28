using Bedrock.BuildingBlocks.Persistence.PostgreSql.Connections.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.UnitOfWork;

/// <summary>
/// Test PostgreSQL unit of work implementation for integration tests.
/// </summary>
public class TestPostgreSqlUnitOfWork : PostgreSqlUnitOfWorkBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestPostgreSqlUnitOfWork"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connection">The PostgreSQL connection.</param>
    public TestPostgreSqlUnitOfWork(
        ILogger<TestPostgreSqlUnitOfWork> logger,
        IPostgreSqlConnection connection)
        : base(logger, "TestUnitOfWork", connection)
    {
    }
}
