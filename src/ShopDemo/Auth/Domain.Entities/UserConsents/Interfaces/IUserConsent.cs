using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.UserConsents.Enums;

namespace ShopDemo.Auth.Domain.Entities.UserConsents.Interfaces;

public interface IUserConsent
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    Id ConsentTermId { get; }
    DateTimeOffset AcceptedAt { get; }
    UserConsentStatus Status { get; }
    DateTimeOffset? RevokedAt { get; }
    string? IpAddress { get; }
}
