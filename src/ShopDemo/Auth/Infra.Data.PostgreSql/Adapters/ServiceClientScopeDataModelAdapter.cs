using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class ServiceClientScopeDataModelAdapter
{
    public static ServiceClientScopeDataModel Adapt(
        ServiceClientScopeDataModel dataModel,
        ServiceClientScope entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.ServiceClientId = entity.ServiceClientId.Value;
        dataModel.Scope = entity.Scope;

        return dataModel;
    }
}
