using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Infra.Persistence.DataModels;

namespace ShopDemo.Auth.Infra.Persistence.Factories;

public static class UserDataModelFactory
{
    public static UserDataModel Create(User entity)
    {
        UserDataModel dataModel =
            DataModelBaseFactory.Create<UserDataModel, User>(entity);

        dataModel.Username = entity.Username;
        dataModel.Email = entity.Email.Value;
        dataModel.PasswordHash = entity.PasswordHash.Value.ToArray();
        dataModel.Status = (short)entity.Status;

        return dataModel;
    }
}
