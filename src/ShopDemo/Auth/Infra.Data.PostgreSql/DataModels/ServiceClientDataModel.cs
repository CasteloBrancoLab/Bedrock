using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class ServiceClientDataModel : DataModelBase
{
    public string ClientId { get; set; } = null!;
    public byte[] ClientSecretHash { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
