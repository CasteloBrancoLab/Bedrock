using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Claims;

namespace ShopDemo.Auth.Domain.Entities.RoleClaims.Interfaces;

public interface IRoleClaim
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id RoleId { get; }
    Id ClaimId { get; }
    ClaimValue Value { get; }
}
