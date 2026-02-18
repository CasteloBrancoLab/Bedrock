using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class ApiKeyDataModel : DataModelBase
{
    public Guid ServiceClientId { get; set; }
    public string KeyPrefix { get; set; } = null!;
    public string KeyHash { get; set; } = null!;
    public short Status { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
