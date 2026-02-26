using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.RoleClaims;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class RoleClaimDataModelAdapter
{
    public static RoleClaimDataModel Adapt(
        RoleClaimDataModel dataModel,
        RoleClaim entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.RoleId = entity.RoleId.Value;
        dataModel.ClaimId = entity.ClaimId.Value;
        dataModel.Value = entity.Value.Value;

        return dataModel;
    }
}
