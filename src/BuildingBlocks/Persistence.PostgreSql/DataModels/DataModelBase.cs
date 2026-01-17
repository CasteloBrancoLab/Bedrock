namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

public class DataModelBase
{
    public Guid Id { get; set; }
    public Guid TenantCode { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public string? LastChangedBy { get; set; }
    public DateTimeOffset? LastChangedAt { get; set; }
    public string? LastChangedExecutionOrigin { get; set; }
    public Guid? LastChangedCorrelationId { get; set; }
    public string? LastChangedBusinessOperationCode { get; set; }
    public long EntityVersion { get; set; }
}
