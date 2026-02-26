using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.Sessions;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class SessionDataModelAdapter
{
    public static SessionDataModel Adapt(
        SessionDataModel dataModel,
        Session entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.UserId = entity.UserId.Value;
        dataModel.RefreshTokenId = entity.RefreshTokenId.Value;
        dataModel.DeviceInfo = entity.DeviceInfo;
        dataModel.IpAddress = entity.IpAddress;
        dataModel.UserAgent = entity.UserAgent;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.Status = (short)entity.Status;
        dataModel.LastActivityAt = entity.LastActivityAt;
        dataModel.RevokedAt = entity.RevokedAt;

        return dataModel;
    }
}
