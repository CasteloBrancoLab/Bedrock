using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class TenantDataModel : DataModelBase
{
    public string Name { get; set; } = null!;
    public string Domain { get; set; } = null!;
    public string SchemaName { get; set; } = null!;
    public short Status { get; set; }
    public short Tier { get; set; }
    public string? DbVersion { get; set; }
}
