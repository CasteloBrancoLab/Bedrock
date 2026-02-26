using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class LoginAttemptDataModel : DataModelBase
{
    public string Username { get; set; } = null!;
    public string? IpAddress { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
}
