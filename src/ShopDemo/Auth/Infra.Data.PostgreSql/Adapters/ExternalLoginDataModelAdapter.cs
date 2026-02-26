using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.ExternalLogins;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class ExternalLoginDataModelAdapter
{
    public static ExternalLoginDataModel Adapt(
        ExternalLoginDataModel dataModel,
        ExternalLogin entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.Provider = entity.Provider.Value;
        dataModel.ProviderUserId = entity.ProviderUserId;
        dataModel.Email = entity.Email;

        return dataModel;
    }
}
