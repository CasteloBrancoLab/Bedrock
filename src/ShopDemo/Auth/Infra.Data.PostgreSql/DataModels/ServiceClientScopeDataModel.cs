using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class ServiceClientScopeDataModel : DataModelBase
{
    public Guid ServiceClientId { get; set; }
    public string Scope { get; set; } = null!;
}
