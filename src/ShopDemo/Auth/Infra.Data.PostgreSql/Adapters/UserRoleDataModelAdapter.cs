using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.UserRoles;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class UserRoleDataModelAdapter
{
    public static UserRoleDataModel Adapt(
        UserRoleDataModel dataModel,
        UserRole entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.RoleId = entity.RoleId.Value;

        return dataModel;
    }
}
