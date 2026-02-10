using Bedrock.BuildingBlocks.Persistence.Abstractions.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;

/// <summary>
/// Interface for PostgreSQL-specific data model repositories.
/// Extends the base data model repository with PostgreSQL-specific capabilities.
/// </summary>
/// <typeparam name="TDataModel">The data model type that extends DataModelBase.</typeparam>
public interface IPostgreSqlDataModelRepository<TDataModel>
    : IDataModelRepository<TDataModel>
    where TDataModel : DataModelBase
{
}
