using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;

public static class DataModelBaseAdapter
{
    public static TDataModel Adapt<TDataModel>(TDataModel dataModel, IEntity entity)
        where TDataModel : DataModelBase
    {
        dataModel.Id = entity.EntityInfo.Id;
        dataModel.TenantCode = entity.EntityInfo.TenantInfo.Code;
        dataModel.CreatedBy = entity.EntityInfo.EntityChangeInfo.CreatedBy;
        dataModel.CreatedAt = entity.EntityInfo.EntityChangeInfo.CreatedAt;
        dataModel.LastChangedBy = entity.EntityInfo.EntityChangeInfo.LastChangedBy;
        dataModel.LastChangedAt = entity.EntityInfo.EntityChangeInfo.LastChangedAt;
        dataModel.LastChangedExecutionOrigin = entity.EntityInfo.EntityChangeInfo.LastChangedExecutionOrigin;
        dataModel.LastChangedCorrelationId = entity.EntityInfo.EntityChangeInfo.LastChangedCorrelationId;
        dataModel.LastChangedBusinessOperationCode = entity.EntityInfo.EntityChangeInfo.LastChangedBusinessOperationCode;
        dataModel.EntityVersion = entity.EntityInfo.EntityVersion;

        return dataModel;
    }
}
