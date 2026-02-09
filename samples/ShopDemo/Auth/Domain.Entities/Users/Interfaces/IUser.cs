using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Auth.Domain.Entities.Users.Enums;

namespace ShopDemo.Auth.Domain.Entities.Users.Interfaces;

public interface IUser : IEntity
{
    string Username { get; }
    EmailAddress Email { get; }
    PasswordHash PasswordHash { get; }
    UserStatus Status { get; }
}
