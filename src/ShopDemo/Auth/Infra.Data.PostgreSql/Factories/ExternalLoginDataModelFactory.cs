using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.ExternalLogins;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class ExternalLoginDataModelFactory
{
    public static ExternalLoginDataModel Create(ExternalLogin entity)
    {
        ExternalLoginDataModel dataModel =
            DataModelBaseFactory.Create<ExternalLoginDataModel, ExternalLogin>(entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.Provider = entity.Provider.Value;
        dataModel.ProviderUserId = entity.ProviderUserId;
        dataModel.Email = entity.Email;

        return dataModel;
    }
}
