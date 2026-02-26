using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class MfaSetupDataModel : DataModelBase
{
    public Guid UserId { get; set; }
    public string EncryptedSharedSecret { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public DateTimeOffset? EnabledAt { get; set; }
}
