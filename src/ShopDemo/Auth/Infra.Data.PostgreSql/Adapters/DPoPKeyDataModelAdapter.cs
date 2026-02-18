using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class DPoPKeyDataModelAdapter
{
    public static DPoPKeyDataModel Adapt(
        DPoPKeyDataModel dataModel,
        DPoPKey entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.JwkThumbprint = entity.JwkThumbprint.Value;
        dataModel.PublicKeyJwk = entity.PublicKeyJwk;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.Status = (short)entity.Status;
        dataModel.RevokedAt = entity.RevokedAt;

        return dataModel;
    }
}
