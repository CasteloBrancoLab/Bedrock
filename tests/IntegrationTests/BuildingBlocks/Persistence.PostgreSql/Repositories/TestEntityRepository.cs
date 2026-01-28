using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Microsoft.Extensions.Logging;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Repositories;

/// <summary>
/// Test entity repository for integration tests.
/// Extends DataModelRepositoryBase to test the base repository functionality.
/// </summary>
public class TestEntityRepository : DataModelRepositoryBase<TestEntityDataModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestEntityRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="unitOfWork">The unit of work for managing database connections and transactions.</param>
    /// <param name="mapper">The data model mapper for SQL generation and property mapping.</param>
    public TestEntityRepository(
        ILogger<TestEntityRepository> logger,
        IPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<TestEntityDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
    }
}
