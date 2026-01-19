using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Templates.Domain.Entities.SimpleAggregateRoots;
using Templates.Domain.Entities.SimpleAggregateRoots.Inputs;
using Templates.Infra.Data.PostgreSql.DataModels;

namespace Templates.Infra.Data.PostgreSql.Factories;

public static class SimpleAggregateRootFactory
{
    public static SimpleAggregateRoot Create(SimpleAggregateRootDataModel dataModel)
    {
        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(dataModel.Id),
            tenantInfo: TenantInfo.Create(dataModel.TenantCode),
            createdAt: dataModel.CreatedAt,
            createdBy: dataModel.CreatedBy,
            createdCorrelationId: Guid.Empty,
            createdExecutionOrigin: string.Empty,
            createdBusinessOperationCode: string.Empty,
            lastChangedAt: dataModel.LastChangedAt,
            lastChangedBy: dataModel.LastChangedBy,
            lastChangedCorrelationId: dataModel.LastChangedCorrelationId,
            lastChangedExecutionOrigin: dataModel.LastChangedExecutionOrigin,
            lastChangedBusinessOperationCode: dataModel.LastChangedBusinessOperationCode,
            entityVersion: RegistryVersion.CreateFromExistingInfo(dataModel.EntityVersion));

        return SimpleAggregateRoot.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(
                entityInfo,
                dataModel.FirstName,
                dataModel.LastName,
                dataModel.FullName,
                BirthDate.CreateNew(dataModel.BirthDate)));
    }
}
