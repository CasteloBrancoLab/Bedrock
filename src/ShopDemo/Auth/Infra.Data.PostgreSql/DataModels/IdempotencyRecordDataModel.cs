using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class IdempotencyRecordDataModel : DataModelBase
{
    public string IdempotencyKey { get; set; } = null!;
    public string RequestHash { get; set; } = null!;
    public string? ResponseBody { get; set; }
    public int StatusCode { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
