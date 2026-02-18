using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class UserConsentDataModel : DataModelBase
{
    public Guid UserId { get; set; }
    public Guid ConsentTermId { get; set; }
    public DateTimeOffset AcceptedAt { get; set; }
    public short Status { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? IpAddress { get; set; }
}
