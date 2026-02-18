using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class ClaimDependencyDataModelFactory
{
    public static ClaimDependencyDataModel Create(ClaimDependency entity)
    {
        ClaimDependencyDataModel dataModel =
            DataModelBaseFactory.Create<ClaimDependencyDataModel, ClaimDependency>(entity);

        dataModel.ClaimId = entity.ClaimId.Value;
        dataModel.DependsOnClaimId = entity.DependsOnClaimId.Value;

        return dataModel;
    }
}
