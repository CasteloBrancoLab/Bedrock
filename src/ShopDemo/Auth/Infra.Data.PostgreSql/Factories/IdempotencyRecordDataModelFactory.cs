using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class IdempotencyRecordDataModelFactory
{
    public static IdempotencyRecordDataModel Create(IdempotencyRecord entity)
    {
        IdempotencyRecordDataModel dataModel =
            DataModelBaseFactory.Create<IdempotencyRecordDataModel, IdempotencyRecord>(entity);

        dataModel.IdempotencyKey = entity.IdempotencyKey;
        dataModel.RequestHash = entity.RequestHash;
        dataModel.ResponseBody = entity.ResponseBody;
        dataModel.StatusCode = entity.StatusCode;
        dataModel.ExpiresAt = entity.ExpiresAt;

        return dataModel;
    }
}
