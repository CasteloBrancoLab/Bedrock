using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class DenyListEntryDataModelFactory
{
    public static DenyListEntryDataModel Create(DenyListEntry entity)
    {
        DenyListEntryDataModel dataModel =
            DataModelBaseFactory.Create<DenyListEntryDataModel, DenyListEntry>(entity);

        dataModel.Type = (short)entity.Type;
        dataModel.Value = entity.Value;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.Reason = entity.Reason;

        return dataModel;
    }
}
