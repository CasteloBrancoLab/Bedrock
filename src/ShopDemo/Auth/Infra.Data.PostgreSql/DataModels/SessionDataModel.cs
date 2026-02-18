using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class SessionDataModel : DataModelBase
{
    public Guid UserId { get; set; }
    public Guid RefreshTokenId { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public short Status { get; set; }
    public DateTimeOffset LastActivityAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
