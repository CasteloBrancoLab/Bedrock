using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class DPoPKeyDataModelFactory
{
    public static DPoPKeyDataModel Create(DPoPKey entity)
    {
        DPoPKeyDataModel dataModel =
            DataModelBaseFactory.Create<DPoPKeyDataModel, DPoPKey>(entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.JwkThumbprint = entity.JwkThumbprint.Value;
        dataModel.PublicKeyJwk = entity.PublicKeyJwk;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.Status = (short)entity.Status;
        dataModel.RevokedAt = entity.RevokedAt;

        return dataModel;
    }
}
