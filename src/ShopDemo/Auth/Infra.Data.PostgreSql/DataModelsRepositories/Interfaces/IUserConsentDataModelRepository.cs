using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IUserConsentDataModelRepository
    : IPostgreSqlDataModelRepository<UserConsentDataModel>
{
    Task<IReadOnlyList<UserConsentDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken);

    Task<UserConsentDataModel?> GetActiveByUserIdAndConsentTermIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        Guid consentTermId,
        CancellationToken cancellationToken);
}
