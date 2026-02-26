using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class ClaimDataModelFactory
{
    public static ClaimDataModel Create(Claim entity)
    {
        ClaimDataModel dataModel =
            DataModelBaseFactory.Create<ClaimDataModel, Claim>(entity);

        dataModel.Name = entity.Name;
        dataModel.Description = entity.Description;

        return dataModel;
    }
}
