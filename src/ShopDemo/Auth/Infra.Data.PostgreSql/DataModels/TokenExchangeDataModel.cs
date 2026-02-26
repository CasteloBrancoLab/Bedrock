using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class TokenExchangeDataModel : DataModelBase
{
    public Guid UserId { get; set; }
    public string SubjectTokenJti { get; set; } = null!;
    public string RequestedAudience { get; set; } = null!;
    public string IssuedTokenJti { get; set; } = null!;
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
