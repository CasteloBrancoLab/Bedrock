using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.Roles;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class RoleDataModelFactory
{
    public static RoleDataModel Create(Role entity)
    {
        RoleDataModel dataModel =
            DataModelBaseFactory.Create<RoleDataModel, Role>(entity);

        dataModel.Name = entity.Name;
        dataModel.Description = entity.Description;

        return dataModel;
    }
}
