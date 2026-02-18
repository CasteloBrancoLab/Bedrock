using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.UserConsents;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class UserConsentDataModelFactory
{
    public static UserConsentDataModel Create(UserConsent entity)
    {
        UserConsentDataModel dataModel =
            DataModelBaseFactory.Create<UserConsentDataModel, UserConsent>(entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.ConsentTermId = entity.ConsentTermId.Value;
        dataModel.AcceptedAt = entity.AcceptedAt;
        dataModel.Status = (short)entity.Status;
        dataModel.RevokedAt = entity.RevokedAt;
        dataModel.IpAddress = entity.IpAddress;

        return dataModel;
    }
}
