namespace ShopDemo.Auth.Domain.Entities.Users.Interfaces;

public interface IUser
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot,
    ShopDemo.Core.Entities.Users.Interfaces.IUser
{
    PasswordHash PasswordHash { get; }
}
