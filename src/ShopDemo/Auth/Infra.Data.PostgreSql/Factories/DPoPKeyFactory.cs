using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class DPoPKeyFactory
{
    public static DPoPKey Create(DPoPKeyDataModel dataModel)
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

        return DPoPKey.CreateFromExistingInfo(
            new CreateFromExistingInfoDPoPKeyInput(
                entityInfo,
                Id.CreateFromExistingInfo(dataModel.UserId),
                JwkThumbprint.CreateFromExistingInfo(dataModel.JwkThumbprint),
                dataModel.PublicKeyJwk,
                dataModel.ExpiresAt,
                (DPoPKeyStatus)dataModel.Status,
                dataModel.RevokedAt));
    }
}
