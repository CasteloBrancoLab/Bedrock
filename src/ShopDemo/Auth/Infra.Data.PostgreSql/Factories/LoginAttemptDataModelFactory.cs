using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.LoginAttempts;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class LoginAttemptDataModelFactory
{
    public static LoginAttemptDataModel Create(LoginAttempt entity)
    {
        LoginAttemptDataModel dataModel =
            DataModelBaseFactory.Create<LoginAttemptDataModel, LoginAttempt>(entity);

        dataModel.Username = entity.Username;
        dataModel.IpAddress = entity.IpAddress;
        dataModel.AttemptedAt = entity.AttemptedAt;
        dataModel.IsSuccessful = entity.IsSuccessful;
        dataModel.FailureReason = entity.FailureReason;

        return dataModel;
    }
}
