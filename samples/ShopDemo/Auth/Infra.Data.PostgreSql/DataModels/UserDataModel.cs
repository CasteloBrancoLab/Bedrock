using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

public class UserDataModel : DataModelBase
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public byte[] PasswordHash { get; set; } = null!;
    public short Status { get; set; }
}
