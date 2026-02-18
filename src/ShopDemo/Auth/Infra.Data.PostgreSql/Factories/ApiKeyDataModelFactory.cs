using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.ApiKeys;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class ApiKeyDataModelFactory
{
    public static ApiKeyDataModel Create(ApiKey entity)
    {
        ApiKeyDataModel dataModel =
            DataModelBaseFactory.Create<ApiKeyDataModel, ApiKey>(entity);

        dataModel.ServiceClientId = entity.ServiceClientId.Value;
        dataModel.KeyPrefix = entity.KeyPrefix;
        dataModel.KeyHash = entity.KeyHash;
        dataModel.Status = (short)entity.Status;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.LastUsedAt = entity.LastUsedAt;
        dataModel.RevokedAt = entity.RevokedAt;

        return dataModel;
    }
}
