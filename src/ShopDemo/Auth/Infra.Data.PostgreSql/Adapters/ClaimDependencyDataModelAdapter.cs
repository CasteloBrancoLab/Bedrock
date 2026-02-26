using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.ClaimDependencies;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class ClaimDependencyDataModelAdapter
{
    public static ClaimDependencyDataModel Adapt(
        ClaimDependencyDataModel dataModel,
        ClaimDependency entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.ClaimId = entity.ClaimId.Value;
        dataModel.DependsOnClaimId = entity.DependsOnClaimId.Value;

        return dataModel;
    }
}
