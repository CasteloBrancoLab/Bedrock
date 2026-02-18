using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.KeyChains;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class KeyChainDataModelAdapter
{
    public static KeyChainDataModel Adapt(
        KeyChainDataModel dataModel,
        KeyChain entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.KeyId = entity.KeyId.Value;
        dataModel.PublicKey = entity.PublicKey;
        dataModel.EncryptedSharedSecret = entity.EncryptedSharedSecret;
        dataModel.Status = (short)entity.Status;
        dataModel.ExpiresAt = entity.ExpiresAt;

        return dataModel;
    }
}
