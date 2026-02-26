using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class RoleHierarchyDataModelFactory
{
    public static RoleHierarchyDataModel Create(RoleHierarchy entity)
    {
        RoleHierarchyDataModel dataModel =
            DataModelBaseFactory.Create<RoleHierarchyDataModel, RoleHierarchy>(entity);

        dataModel.RoleId = entity.RoleId.Value;
        dataModel.ParentRoleId = entity.ParentRoleId.Value;

        return dataModel;
    }
}
