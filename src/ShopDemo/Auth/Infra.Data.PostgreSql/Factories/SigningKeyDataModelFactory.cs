using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.SigningKeys;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class SigningKeyDataModelFactory
{
    public static SigningKeyDataModel Create(SigningKey entity)
    {
        SigningKeyDataModel dataModel =
            DataModelBaseFactory.Create<SigningKeyDataModel, SigningKey>(entity);

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
