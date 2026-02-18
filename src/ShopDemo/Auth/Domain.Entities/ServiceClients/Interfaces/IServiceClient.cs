using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;

namespace ShopDemo.Auth.Domain.Entities.ServiceClients.Interfaces;

public interface IServiceClient
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    string ClientId { get; }
    byte[] ClientSecretHash { get; }
    string Name { get; }
    ServiceClientStatus Status { get; }
    Id CreatedByUserId { get; }
    DateTimeOffset? ExpiresAt { get; }
    DateTimeOffset? RevokedAt { get; }
}
