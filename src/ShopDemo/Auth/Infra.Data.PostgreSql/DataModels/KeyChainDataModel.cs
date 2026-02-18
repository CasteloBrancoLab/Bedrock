using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class KeyChainDataModel : DataModelBase
{
    public Guid UserId { get; set; }
    public string KeyId { get; set; } = null!;
    public string PublicKey { get; set; } = null!;
    public string EncryptedSharedSecret { get; set; } = null!;
    public short Status { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
