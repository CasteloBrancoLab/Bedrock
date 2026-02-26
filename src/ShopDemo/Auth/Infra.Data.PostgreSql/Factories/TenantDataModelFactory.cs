using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.Tenants;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class TenantDataModelFactory
{
    public static TenantDataModel Create(Tenant entity)
    {
        TenantDataModel dataModel =
            DataModelBaseFactory.Create<TenantDataModel, Tenant>(entity);

        dataModel.Name = entity.Name;
        dataModel.Domain = entity.Domain;
        dataModel.SchemaName = entity.SchemaName;
        dataModel.Status = (short)entity.Status;
        dataModel.Tier = (short)entity.Tier;
        dataModel.DbVersion = entity.DbVersion;

        return dataModel;
    }
}
