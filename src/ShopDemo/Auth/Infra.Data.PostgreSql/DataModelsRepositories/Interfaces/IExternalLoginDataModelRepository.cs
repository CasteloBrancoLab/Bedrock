using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IExternalLoginDataModelRepository
    : IPostgreSqlDataModelRepository<ExternalLoginDataModel>
{
    Task<ExternalLoginDataModel?> GetByProviderAndProviderUserIdAsync(
        ExecutionContext executionContext,
        string provider,
        string providerUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ExternalLoginDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> DeleteByUserIdAndProviderAsync(
        ExecutionContext executionContext,
        Guid userId,
        string provider,
        CancellationToken cancellationToken);
}
