using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class ConsentTermDataModel : DataModelBase
{
    public short Type { get; set; }
    public string Version { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTimeOffset PublishedAt { get; set; }
}
