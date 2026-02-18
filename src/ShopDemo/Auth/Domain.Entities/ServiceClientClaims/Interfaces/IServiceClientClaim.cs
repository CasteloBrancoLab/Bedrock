using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Claims;

namespace ShopDemo.Auth.Domain.Entities.ServiceClientClaims.Interfaces;

public interface IServiceClientClaim
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id ServiceClientId { get; }
    Id ClaimId { get; }
    ClaimValue Value { get; }
}
