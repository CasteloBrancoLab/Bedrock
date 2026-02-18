using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class SigningKeyDataModel : DataModelBase
{
    public string Kid { get; set; } = null!;
    public string Algorithm { get; set; } = null!;
    public string PublicKey { get; set; } = null!;
    public string EncryptedPrivateKey { get; set; } = null!;
    public short Status { get; set; }
    public DateTimeOffset? RotatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
