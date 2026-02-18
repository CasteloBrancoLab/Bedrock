using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.RoleHierarchies;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class RoleHierarchyDataModelAdapter
{
    public static RoleHierarchyDataModel Adapt(
        RoleHierarchyDataModel dataModel,
        RoleHierarchy entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.RoleId = entity.RoleId.Value;
        dataModel.ParentRoleId = entity.ParentRoleId.Value;

        return dataModel;
    }
}
