using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using Templates.Infra.Data.PostgreSql.DataModels;

namespace Templates.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

/// <summary>
/// Repository interface for SimpleAggregateRootDataModel persistence operations.
/// </summary>
public interface ISimpleAggregateRootDataModelRepository
    : IPostgreSqlDataModelRepository<SimpleAggregateRootDataModel>
{
}
