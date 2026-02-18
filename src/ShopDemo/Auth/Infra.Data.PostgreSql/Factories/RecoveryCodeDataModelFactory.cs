using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class RecoveryCodeDataModelFactory
{
    public static RecoveryCodeDataModel Create(RecoveryCode entity)
    {
        RecoveryCodeDataModel dataModel =
            DataModelBaseFactory.Create<RecoveryCodeDataModel, RecoveryCode>(entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.CodeHash = entity.CodeHash;
        dataModel.IsUsed = entity.IsUsed;
        dataModel.UsedAt = entity.UsedAt;

        return dataModel;
    }
}
