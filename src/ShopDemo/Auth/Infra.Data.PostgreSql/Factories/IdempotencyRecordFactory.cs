using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords;
using ShopDemo.Auth.Domain.Entities.IdempotencyRecords.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class IdempotencyRecordFactory
{
    public static IdempotencyRecord Create(IdempotencyRecordDataModel dataModel)
    {
        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(dataModel.Id),
            tenantInfo: TenantInfo.Create(dataModel.TenantCode),
            createdAt: dataModel.CreatedAt,
            createdBy: dataModel.CreatedBy,
            createdCorrelationId: dataModel.CreatedCorrelationId,
            createdExecutionOrigin: dataModel.CreatedExecutionOrigin,
            createdBusinessOperationCode: dataModel.CreatedBusinessOperationCode,
            lastChangedAt: dataModel.LastChangedAt,
            lastChangedBy: dataModel.LastChangedBy,
            lastChangedCorrelationId: dataModel.LastChangedCorrelationId,
            lastChangedExecutionOrigin: dataModel.LastChangedExecutionOrigin,
            lastChangedBusinessOperationCode: dataModel.LastChangedBusinessOperationCode,
            entityVersion: RegistryVersion.CreateFromExistingInfo(dataModel.EntityVersion));

        return IdempotencyRecord.CreateFromExistingInfo(
            new CreateFromExistingInfoIdempotencyRecordInput(
                entityInfo,
                dataModel.IdempotencyKey,
                dataModel.RequestHash,
                dataModel.ResponseBody,
                dataModel.StatusCode,
                dataModel.ExpiresAt));
    }
}
