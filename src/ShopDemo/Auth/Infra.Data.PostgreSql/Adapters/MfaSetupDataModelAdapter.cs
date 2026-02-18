using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.MfaSetups;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class MfaSetupDataModelAdapter
{
    public static MfaSetupDataModel Adapt(
        MfaSetupDataModel dataModel,
        MfaSetup entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.EncryptedSharedSecret = entity.EncryptedSharedSecret;
        dataModel.IsEnabled = entity.IsEnabled;
        dataModel.EnabledAt = entity.EnabledAt;

        return dataModel;
    }
}
