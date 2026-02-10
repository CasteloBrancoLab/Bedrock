namespace ShopDemo.Auth.Domain.Entities.Users.Interfaces;

public interface IUser
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot,
    ShopDemo.Core.Entities.Users.IUser
{
    PasswordHash PasswordHash { get; }
}
