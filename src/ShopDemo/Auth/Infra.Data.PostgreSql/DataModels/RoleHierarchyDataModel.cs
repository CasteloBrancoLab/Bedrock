using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class RoleHierarchyDataModel : DataModelBase
{
    public Guid RoleId { get; set; }
    public Guid ParentRoleId { get; set; }
}
