using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using Templates.Infra.Data.PostgreSql.DataModels;

namespace Templates.Infra.Data.PostgreSql.Repositories.Interfaces;

/// <summary>
/// Repository interface for SimpleAggregateRootDataModel persistence operations.
/// </summary>
public interface ISimpleAggregateRootDataModelRepository
    : IPostgreSqlDataModelRepository<SimpleAggregateRootDataModel>
{
}
