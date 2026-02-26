using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.TokenExchanges;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class TokenExchangeDataModelAdapter
{
    public static TokenExchangeDataModel Adapt(
        TokenExchangeDataModel dataModel,
        TokenExchange entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.SubjectTokenJti = entity.SubjectTokenJti;
        dataModel.RequestedAudience = entity.RequestedAudience;
        dataModel.IssuedTokenJti = entity.IssuedTokenJti;
        dataModel.IssuedAt = entity.IssuedAt;
        dataModel.ExpiresAt = entity.ExpiresAt;

        return dataModel;
    }
}
