using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.RoleClaims;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class RoleClaimDataModelFactory
{
    public static RoleClaimDataModel Create(RoleClaim entity)
    {
        RoleClaimDataModel dataModel =
            DataModelBaseFactory.Create<RoleClaimDataModel, RoleClaim>(entity);

        dataModel.RoleId = entity.RoleId.Value;
        dataModel.ClaimId = entity.ClaimId.Value;
        dataModel.Value = entity.Value.Value;

        return dataModel;
    }
}
