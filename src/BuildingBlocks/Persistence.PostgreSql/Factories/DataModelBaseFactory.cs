using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;

public static class DataModelBaseFactory
{
    public static TDataModel Create<TDataModel, TEntity>(TEntity entity)
        where TDataModel : DataModelBase, new()
        where TEntity : IEntity
    {
        TDataModel dataModel = new()
        {
            Id = entity.EntityInfo.Id,
            TenantCode = entity.EntityInfo.TenantInfo.Code,
            CreatedBy = entity.EntityInfo.EntityChangeInfo.CreatedBy,
            CreatedAt = entity.EntityInfo.EntityChangeInfo.CreatedAt,
            LastChangedBy = entity.EntityInfo.EntityChangeInfo.LastChangedBy,
            LastChangedAt = entity.EntityInfo.EntityChangeInfo.LastChangedAt,
            LastChangedExecutionOrigin = entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin,
            LastChangedCorrelationId = entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId,
            LastChangedBusinessOperationCode = entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode,
            EntityVersion = entity.EntityInfo.EntityVersion
        };

        return dataModel;
    }

    public static TDataModel Create<TDataModel>(ExecutionContext executionContext)
        where TDataModel : DataModelBase, new()
    {
        TDataModel dataModel = new()
        {
            TenantCode = executionContext.TenantInfo.Code,
            CreatedBy = executionContext.ExecutionUser,
            CreatedAt = executionContext.Timestamp,
            LastChangedBy = null,
            LastChangedAt = null,
            LastChangedExecutionOrigin = null,
            LastChangedCorrelationId = null,
            LastChangedBusinessOperationCode = null,
            EntityVersion = executionContext.Timestamp.Ticks
        };

        return dataModel;
    }
}
