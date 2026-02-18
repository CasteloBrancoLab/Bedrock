using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.PasswordResetTokens;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class PasswordResetTokenDataModelFactory
{
    public static PasswordResetTokenDataModel Create(PasswordResetToken entity)
    {
        PasswordResetTokenDataModel dataModel =
            DataModelBaseFactory.Create<PasswordResetTokenDataModel, PasswordResetToken>(entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.TokenHash = entity.TokenHash;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.IsUsed = entity.IsUsed;
        dataModel.UsedAt = entity.UsedAt;

        return dataModel;
    }
}
