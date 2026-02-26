using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class DenyListEntryDataModel : DataModelBase
{
    public short Type { get; set; }
    public string Value { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public string? Reason { get; set; }
}
