using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.PasswordHistories;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class PasswordHistoryDataModelAdapter
{
    public static PasswordHistoryDataModel Adapt(
        PasswordHistoryDataModel dataModel,
        PasswordHistory entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.PasswordHash = entity.PasswordHash;
        dataModel.ChangedAt = entity.ChangedAt;

        return dataModel;
    }
}
