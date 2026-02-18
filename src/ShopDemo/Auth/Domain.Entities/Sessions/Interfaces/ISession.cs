using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;

namespace ShopDemo.Auth.Domain.Entities.Sessions.Interfaces;

public interface ISession
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    Id RefreshTokenId { get; }
    string? DeviceInfo { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    DateTimeOffset ExpiresAt { get; }
    SessionStatus Status { get; }
    DateTimeOffset LastActivityAt { get; }
    DateTimeOffset? RevokedAt { get; }
}
