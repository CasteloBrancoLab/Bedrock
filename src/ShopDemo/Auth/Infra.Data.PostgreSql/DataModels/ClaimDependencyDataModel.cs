using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class ClaimDependencyDataModel : DataModelBase
{
    public Guid ClaimId { get; set; }
    public Guid DependsOnClaimId { get; set; }
}
