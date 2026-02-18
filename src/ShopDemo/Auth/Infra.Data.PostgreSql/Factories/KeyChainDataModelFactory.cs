using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.KeyChains;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class KeyChainDataModelFactory
{
    public static KeyChainDataModel Create(KeyChain entity)
    {
        KeyChainDataModel dataModel =
            DataModelBaseFactory.Create<KeyChainDataModel, KeyChain>(entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.KeyId = entity.KeyId.Value;
        dataModel.PublicKey = entity.PublicKey;
        dataModel.EncryptedSharedSecret = entity.EncryptedSharedSecret;
        dataModel.Status = (short)entity.Status;
        dataModel.ExpiresAt = entity.ExpiresAt;

        return dataModel;
    }
}
