using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.MfaSetups;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class MfaSetupDataModelFactory
{
    public static MfaSetupDataModel Create(MfaSetup entity)
    {
        MfaSetupDataModel dataModel =
            DataModelBaseFactory.Create<MfaSetupDataModel, MfaSetup>(entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.EncryptedSharedSecret = entity.EncryptedSharedSecret;
        dataModel.IsEnabled = entity.IsEnabled;
        dataModel.EnabledAt = entity.EnabledAt;

        return dataModel;
    }
}
