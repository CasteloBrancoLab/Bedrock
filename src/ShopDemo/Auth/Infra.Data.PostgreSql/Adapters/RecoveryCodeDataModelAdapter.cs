using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.RecoveryCodes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class RecoveryCodeDataModelAdapter
{
    public static RecoveryCodeDataModel Adapt(
        RecoveryCodeDataModel dataModel,
        RecoveryCode entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.CodeHash = entity.CodeHash;
        dataModel.IsUsed = entity.IsUsed;
        dataModel.UsedAt = entity.UsedAt;

        return dataModel;
    }
}
