using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class ImpersonationSessionDataModelFactory
{
    public static ImpersonationSessionDataModel Create(ImpersonationSession entity)
    {
        ImpersonationSessionDataModel dataModel =
            DataModelBaseFactory.Create<ImpersonationSessionDataModel, ImpersonationSession>(entity);

        dataModel.OperatorUserId = entity.OperatorUserId.Value;
        dataModel.TargetUserId = entity.TargetUserId.Value;
        dataModel.ExpiresAt = entity.ExpiresAt;
        dataModel.Status = (short)entity.Status;
        dataModel.EndedAt = entity.EndedAt;

        return dataModel;
    }
}
