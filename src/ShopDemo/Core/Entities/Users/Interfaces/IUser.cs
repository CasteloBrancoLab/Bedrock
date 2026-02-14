using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Users.Enums;

namespace ShopDemo.Core.Entities.Users.Interfaces;

public interface IUser : IEntity
{
    string Username { get; }
    EmailAddress Email { get; }
    UserStatus Status { get; }
}
