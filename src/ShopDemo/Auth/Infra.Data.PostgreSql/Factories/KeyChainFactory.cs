using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.KeyChains;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;
using ShopDemo.Auth.Domain.Entities.KeyChains.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class KeyChainFactory
{
    public static KeyChain Create(KeyChainDataModel dataModel)
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

        return KeyChain.CreateFromExistingInfo(
            new CreateFromExistingInfoKeyChainInput(
                entityInfo,
                Id.CreateFromExistingInfo(dataModel.UserId),
                KeyId.CreateFromExistingInfo(dataModel.KeyId),
                dataModel.PublicKey,
                dataModel.EncryptedSharedSecret,
                (KeyChainStatus)dataModel.Status,
                dataModel.ExpiresAt));
    }
}
