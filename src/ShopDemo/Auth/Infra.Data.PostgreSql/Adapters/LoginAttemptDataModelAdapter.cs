using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.LoginAttempts;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class LoginAttemptDataModelAdapter
{
    public static LoginAttemptDataModel Adapt(
        LoginAttemptDataModel dataModel,
        LoginAttempt entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.Username = entity.Username;
        dataModel.IpAddress = entity.IpAddress;
        dataModel.AttemptedAt = entity.AttemptedAt;
        dataModel.IsSuccessful = entity.IsSuccessful;
        dataModel.FailureReason = entity.FailureReason;

        return dataModel;
    }
}
