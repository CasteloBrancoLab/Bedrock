using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.ServiceClientScopes;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class ServiceClientScopeDataModelFactory
{
    public static ServiceClientScopeDataModel Create(ServiceClientScope entity)
    {
        ServiceClientScopeDataModel dataModel =
            DataModelBaseFactory.Create<ServiceClientScopeDataModel, ServiceClientScope>(entity);

        dataModel.ServiceClientId = entity.ServiceClientId.Value;
        dataModel.Scope = entity.Scope;

        return dataModel;
    }
}
