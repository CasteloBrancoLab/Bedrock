using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.Roles;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class RoleDataModelAdapter
{
    public static RoleDataModel Adapt(
        RoleDataModel dataModel,
        Role entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.Name = entity.Name;
        dataModel.Description = entity.Description;

        return dataModel;
    }
}
