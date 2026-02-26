using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.PasswordHistories.Interfaces;

public interface IPasswordHistory
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    string PasswordHash { get; }
    DateTimeOffset ChangedAt { get; }
}
