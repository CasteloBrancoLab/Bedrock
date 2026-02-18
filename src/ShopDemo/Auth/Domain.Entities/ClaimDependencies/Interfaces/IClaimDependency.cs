using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.ClaimDependencies.Interfaces;

public interface IClaimDependency
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id ClaimId { get; }
    Id DependsOnClaimId { get; }
}
