using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class ImpersonationSessionDataModelAdapter
{
    public static ImpersonationSessionDataModel Adapt(
        ImpersonationSessionDataModel dataModel,
        ImpersonationSession entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.OperatorUserId = entity.OperatorUserId.Value;
        dataModel.TargetUserId = entity.TargetUserId.Value;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.Status = (short)entity.Status;
        dataModel.EndedAt = entity.EndedAt;

        return dataModel;
    }
}
