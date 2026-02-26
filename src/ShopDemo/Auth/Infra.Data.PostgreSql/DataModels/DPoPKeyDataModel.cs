using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class DPoPKeyDataModel : DataModelBase
{
    public Guid UserId { get; set; }
    public string JwkThumbprint { get; set; } = null!;
    public string PublicKeyJwk { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public short Status { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
