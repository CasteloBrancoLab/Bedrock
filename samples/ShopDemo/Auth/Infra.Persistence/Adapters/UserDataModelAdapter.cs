using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Infra.Persistence.DataModels;

namespace ShopDemo.Auth.Infra.Persistence.Adapters;

public static class UserDataModelAdapter
{
    public static UserDataModel Adapt(
        UserDataModel dataModel,
        User entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.Username = entity.Username;
        dataModel.Email = entity.Email.Value;
        dataModel.PasswordHash = entity.PasswordHash.Value.ToArray();
        dataModel.Status = (short)entity.Status;

        return dataModel;
    }
}
