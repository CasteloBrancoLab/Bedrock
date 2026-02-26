using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class UserRoleDataModel : DataModelBase
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
