using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class ServiceClientDataModelFactory
{
    public static ServiceClientDataModel Create(ServiceClient entity)
    {
        ServiceClientDataModel dataModel =
            DataModelBaseFactory.Create<ServiceClientDataModel, ServiceClient>(entity);

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
