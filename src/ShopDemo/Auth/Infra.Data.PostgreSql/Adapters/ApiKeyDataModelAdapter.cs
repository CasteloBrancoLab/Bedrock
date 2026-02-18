using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.ApiKeys;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class ApiKeyDataModelAdapter
{
    public static ApiKeyDataModel Adapt(
        ApiKeyDataModel dataModel,
        ApiKey entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

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
