using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class PasswordResetTokenDataModelAdapter
{
    public static PasswordResetTokenDataModel Adapt(
        PasswordResetTokenDataModel dataModel,
        PasswordResetToken entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.TokenHash = entity.TokenHash;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.IsUsed = entity.IsUsed;
        dataModel.UsedAt = entity.UsedAt;

        return dataModel;
    }
}
