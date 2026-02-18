using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class DenyListEntryDataModelAdapter
{
    public static DenyListEntryDataModel Adapt(
        DenyListEntryDataModel dataModel,
        DenyListEntry entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.Type = (short)entity.Type;
        dataModel.Value = entity.Value;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.Reason = entity.Reason;

        return dataModel;
    }
}
