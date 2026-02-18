using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class PasswordHistoryDataModel : DataModelBase
{
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = null!;
    public DateTimeOffset ChangedAt { get; set; }
}
