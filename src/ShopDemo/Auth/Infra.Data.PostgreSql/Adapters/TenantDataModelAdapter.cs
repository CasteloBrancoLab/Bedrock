using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.Tenants;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class TenantDataModelAdapter
{
    public static TenantDataModel Adapt(
        TenantDataModel dataModel,
        Tenant entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.Name = entity.Name;
        dataModel.Domain = entity.Domain;
        dataModel.SchemaName = entity.SchemaName;
        dataModel.Status = (short)entity.Status;
        dataModel.Tier = (short)entity.Tier;
        dataModel.DbVersion = entity.DbVersion;

        return dataModel;
    }
}
