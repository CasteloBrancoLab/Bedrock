using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface ISigningKeyDataModelRepository
    : IPostgreSqlDataModelRepository<SigningKeyDataModel>
{
    Task<SigningKeyDataModel?> GetActiveAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);

    Task<SigningKeyDataModel?> GetByKidAsync(
        ExecutionContext executionContext,
        string kid,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SigningKeyDataModel>> GetAllValidAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
