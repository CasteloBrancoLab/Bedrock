using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.UserConsents;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class UserConsentDataModelAdapter
{
    public static UserConsentDataModel Adapt(
        UserConsentDataModel dataModel,
        UserConsent entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.ConsentTermId = entity.ConsentTermId.Value;
        dataModel.AcceptedAt = entity.AcceptedAt;
        dataModel.Status = (short)entity.Status;
        dataModel.RevokedAt = entity.RevokedAt;
        dataModel.IpAddress = entity.IpAddress;

        return dataModel;
    }
}
