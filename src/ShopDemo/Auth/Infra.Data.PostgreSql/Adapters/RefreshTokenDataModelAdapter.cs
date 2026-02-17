using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class RefreshTokenDataModelAdapter
{
    public static RefreshTokenDataModel Adapt(
        RefreshTokenDataModel dataModel,
        RefreshToken entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.TokenHash = entity.TokenHash.Value.ToArray();
        dataModel.FamilyId = entity.FamilyId.Value;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.Status = (short)entity.Status;
        dataModel.RevokedAt = entity.RevokedAt;
        dataModel.ReplacedByTokenId = entity.ReplacedByTokenId?.Value;

        return dataModel;
    }
}
