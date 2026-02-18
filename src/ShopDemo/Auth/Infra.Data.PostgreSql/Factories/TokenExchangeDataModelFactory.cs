using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class TokenExchangeDataModelFactory
{
    public static TokenExchangeDataModel Create(TokenExchange entity)
    {
        TokenExchangeDataModel dataModel =
            DataModelBaseFactory.Create<TokenExchangeDataModel, TokenExchange>(entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.SubjectTokenJti = entity.SubjectTokenJti;
        dataModel.RequestedAudience = entity.RequestedAudience;
        dataModel.IssuedTokenJti = entity.IssuedTokenJti;
        dataModel.IssuedAt = entity.IssuedAt;
        dataModel.ExpiresAt = entity.ExpiresAt;

        return dataModel;
    }
}
