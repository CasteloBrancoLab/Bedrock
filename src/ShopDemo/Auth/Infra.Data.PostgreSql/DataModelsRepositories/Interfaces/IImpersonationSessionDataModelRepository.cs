using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface IImpersonationSessionDataModelRepository
    : IPostgreSqlDataModelRepository<ImpersonationSessionDataModel>
{
    Task<ImpersonationSessionDataModel?> GetActiveByOperatorUserIdAsync(
        ExecutionContext executionContext,
        Guid operatorUserId,
        CancellationToken cancellationToken);

    Task<ImpersonationSessionDataModel?> GetActiveByTargetUserIdAsync(
        ExecutionContext executionContext,
        Guid targetUserId,
        CancellationToken cancellationToken);
}
