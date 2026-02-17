using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class RefreshTokenFactory
{
    public static RefreshToken Create(RefreshTokenDataModel dataModel)
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

        return RefreshToken.CreateFromExistingInfo(
            new CreateFromExistingInfoRefreshTokenInput(
                entityInfo,
                Id.CreateFromExistingInfo(dataModel.UserId),
                TokenHash.CreateNew(dataModel.TokenHash),
                TokenFamily.CreateFromExistingInfo(dataModel.FamilyId),
                dataModel.ExpiresAt,
                (RefreshTokenStatus)dataModel.Status,
                dataModel.RevokedAt,
                dataModel.ReplacedByTokenId.HasValue
                    ? Id.CreateFromExistingInfo(dataModel.ReplacedByTokenId.Value)
                    : null));
    }
}
