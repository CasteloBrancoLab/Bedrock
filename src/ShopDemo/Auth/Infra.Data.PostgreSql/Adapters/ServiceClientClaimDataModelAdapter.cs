using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.ServiceClientClaims;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class ServiceClientClaimDataModelAdapter
{
    public static ServiceClientClaimDataModel Adapt(
        ServiceClientClaimDataModel dataModel,
        ServiceClientClaim entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.ServiceClientId = entity.ServiceClientId.Value;
        dataModel.ClaimId = entity.ClaimId.Value;
        dataModel.Value = entity.Value.Value;

        return dataModel;
    }
}
