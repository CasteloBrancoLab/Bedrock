using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace Templates.Domain.Entities.AssociatedAggregateRoots.Inputs;

public readonly record struct PrimaryCreateFromExistingInfoInput(
    EntityInfo EntityInfo,
    int Quantity,
    ReferencedAggregateRoot? ReferencedAggregateRoot
);
