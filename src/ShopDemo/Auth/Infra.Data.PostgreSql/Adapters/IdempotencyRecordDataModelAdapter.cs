using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class IdempotencyRecordDataModelAdapter
{
    public static IdempotencyRecordDataModel Adapt(
        IdempotencyRecordDataModel dataModel,
        IdempotencyRecord entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.IdempotencyKey = entity.IdempotencyKey;
        dataModel.RequestHash = entity.RequestHash;
        dataModel.ResponseBody = entity.ResponseBody;
        dataModel.StatusCode = entity.StatusCode;
        dataModel.ExpiresAt = entity.ExpiresAt;

        return dataModel;
    }
}
