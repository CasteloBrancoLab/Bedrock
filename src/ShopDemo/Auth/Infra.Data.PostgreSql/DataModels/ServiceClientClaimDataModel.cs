using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class ServiceClientClaimDataModel : DataModelBase
{
    public Guid ServiceClientId { get; set; }
    public Guid ClaimId { get; set; }
    public short Value { get; set; }
}
