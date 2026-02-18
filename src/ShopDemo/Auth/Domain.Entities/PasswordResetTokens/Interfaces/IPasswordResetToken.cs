using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.PasswordResetTokens.Interfaces;

public interface IPasswordResetToken
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    string TokenHash { get; }
    DateTimeOffset ExpiresAt { get; }
    bool IsUsed { get; }
    DateTimeOffset? UsedAt { get; }
}
