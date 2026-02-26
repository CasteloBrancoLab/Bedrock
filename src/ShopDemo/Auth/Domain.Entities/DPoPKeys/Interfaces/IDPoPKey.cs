using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;

namespace ShopDemo.Auth.Domain.Entities.DPoPKeys.Interfaces;

public interface IDPoPKey
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    JwkThumbprint JwkThumbprint { get; }
    string PublicKeyJwk { get; }
    DateTimeOffset ExpiresAt { get; }
    DPoPKeyStatus Status { get; }
    DateTimeOffset? RevokedAt { get; }
}
