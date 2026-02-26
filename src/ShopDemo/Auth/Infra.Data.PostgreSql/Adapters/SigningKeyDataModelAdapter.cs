using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.SigningKeys;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class SigningKeyDataModelAdapter
{
    public static SigningKeyDataModel Adapt(
        SigningKeyDataModel dataModel,
        SigningKey entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.Kid = entity.Kid.Value;
        dataModel.Algorithm = entity.Algorithm;
        dataModel.PublicKey = entity.PublicKey;
        dataModel.EncryptedPrivateKey = entity.EncryptedPrivateKey;
        dataModel.Status = (short)entity.Status;
        dataModel.RotatedAt = entity.RotatedAt;
        dataModel.ExpiresAt = entity.ExpiresAt;

        return dataModel;
    }
}
