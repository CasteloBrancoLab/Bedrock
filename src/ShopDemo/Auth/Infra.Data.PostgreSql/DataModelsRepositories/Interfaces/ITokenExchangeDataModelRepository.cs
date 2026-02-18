using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModelRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;

public interface ITokenExchangeDataModelRepository
    : IPostgreSqlDataModelRepository<TokenExchangeDataModel>
{
    Task<IReadOnlyList<TokenExchangeDataModel>> GetByUserIdAsync(
        ExecutionContext executionContext,
        Guid userId,
        CancellationToken cancellationToken);

    Task<TokenExchangeDataModel?> GetByIssuedTokenJtiAsync(
        ExecutionContext executionContext,
        string issuedTokenJti,
        CancellationToken cancellationToken);
}
