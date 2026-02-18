using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class ImpersonationSessionDataModel : DataModelBase
{
    public Guid OperatorUserId { get; set; }
    public Guid TargetUserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public short Status { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
}
