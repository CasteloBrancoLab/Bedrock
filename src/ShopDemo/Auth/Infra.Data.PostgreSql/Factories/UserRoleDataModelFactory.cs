using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.UserRoles;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class UserRoleDataModelFactory
{
    public static UserRoleDataModel Create(UserRole entity)
    {
        UserRoleDataModel dataModel =
            DataModelBaseFactory.Create<UserRoleDataModel, UserRole>(entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.RoleId = entity.RoleId.Value;

        return dataModel;
    }
}
