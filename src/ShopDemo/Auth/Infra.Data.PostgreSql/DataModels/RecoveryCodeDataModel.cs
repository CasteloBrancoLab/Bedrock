using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class RecoveryCodeDataModel : DataModelBase
{
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = null!;
    public bool IsUsed { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
}
