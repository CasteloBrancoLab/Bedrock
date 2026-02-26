using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class ServiceClientClaimDataModelFactory
{
    public static ServiceClientClaimDataModel Create(ServiceClientClaim entity)
    {
        ServiceClientClaimDataModel dataModel =
            DataModelBaseFactory.Create<ServiceClientClaimDataModel, ServiceClientClaim>(entity);

        dataModel.ServiceClientId = entity.ServiceClientId.Value;
        dataModel.ClaimId = entity.ClaimId.Value;
        dataModel.Value = entity.Value.Value;

        return dataModel;
    }
}
