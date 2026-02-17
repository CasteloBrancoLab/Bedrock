using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;

namespace ShopDemo.Auth.Domain.Entities.RefreshTokens.Interfaces;

public interface IRefreshToken
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    TokenHash TokenHash { get; }
    TokenFamily FamilyId { get; }
    DateTimeOffset ExpiresAt { get; }
    RefreshTokenStatus Status { get; }
    DateTimeOffset? RevokedAt { get; }
    Id? ReplacedByTokenId { get; }
}
