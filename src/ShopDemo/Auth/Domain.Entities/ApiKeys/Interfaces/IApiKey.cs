using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;

namespace ShopDemo.Auth.Domain.Entities.ApiKeys.Interfaces;

public interface IApiKey
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id ServiceClientId { get; }
    string KeyPrefix { get; }
    string KeyHash { get; }
    ApiKeyStatus Status { get; }
    DateTimeOffset? ExpiresAt { get; }
    DateTimeOffset? LastUsedAt { get; }
    DateTimeOffset? RevokedAt { get; }
}
