using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class RefreshTokenDataModelFactory
{
    public static RefreshTokenDataModel Create(RefreshToken entity)
    {
        RefreshTokenDataModel dataModel =
            DataModelBaseFactory.Create<RefreshTokenDataModel, RefreshToken>(entity);

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
