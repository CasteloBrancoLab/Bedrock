using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class ServiceClientDataModelAdapter
{
    public static ServiceClientDataModel Adapt(
        ServiceClientDataModel dataModel,
        ServiceClient entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.ClientId = entity.ClientId;
        dataModel.ClientSecretHash = entity.ClientSecretHash;
        dataModel.Name = entity.Name;
        dataModel.Status = (short)entity.Status;
        dataModel.CreatedByUserId = entity.CreatedByUserId.Value;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.RevokedAt = entity.RevokedAt;

        return dataModel;
    }
}
