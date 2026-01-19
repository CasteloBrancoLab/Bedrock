using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories;
using Microsoft.Extensions.Logging;
using Templates.Infra.Data.PostgreSql.DataModels;
using Templates.Infra.Data.PostgreSql.Repositories.Interfaces;
using Templates.Infra.Data.PostgreSql.UnitOfWork.Interfaces;

namespace Templates.Infra.Data.PostgreSql.Repositories;

/// <summary>
/// Repository implementation for SimpleAggregateRootDataModel persistence operations.
/// Provides CRUD operations for SimpleAggregateRoot data models using PostgreSQL.
/// </summary>
/// <remarks>
/// This repository uses the base class functionality for all CRUD operations.
/// The mapper (SimpleAggregateRootDataModelMapper) handles all column mappings,
/// so no additional property configuration is needed.
/// </remarks>
public sealed class SimpleAggregateRootDataModelRepository
    : DataModelRepositoryBase<SimpleAggregateRootDataModel>,
      ISimpleAggregateRootDataModelRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleAggregateRootDataModelRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for recording operations and errors.</param>
    /// <param name="unitOfWork">The unit of work for managing database connections and transactions.</param>
    /// <param name="mapper">The data model mapper for SQL generation and property mapping.</param>
    public SimpleAggregateRootDataModelRepository(
        ILogger<SimpleAggregateRootDataModelRepository> logger,
        ITemplatesPostgreSqlUnitOfWork unitOfWork,
        IDataModelMapper<SimpleAggregateRootDataModel> mapper)
        : base(logger, unitOfWork, mapper)
    {
    }
}
