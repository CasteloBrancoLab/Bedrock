using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.PasswordHistories;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class PasswordHistoryDataModelFactory
{
    public static PasswordHistoryDataModel Create(PasswordHistory entity)
    {
        PasswordHistoryDataModel dataModel =
            DataModelBaseFactory.Create<PasswordHistoryDataModel, PasswordHistory>(entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.PasswordHash = entity.PasswordHash;
        dataModel.ChangedAt = entity.ChangedAt;

        return dataModel;
    }
}
