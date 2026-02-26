using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class ClaimDataModel : DataModelBase
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
