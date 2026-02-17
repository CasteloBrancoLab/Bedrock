using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class RefreshTokenDataModel : DataModelBase
{
    public Guid UserId { get; set; }
    public byte[] TokenHash { get; set; } = null!;
    public Guid FamilyId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public short Status { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
}
