using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace Templates.Domain.Entities.CompositeAggregateRoots.Inputs;

public readonly record struct ChildCreateFromExistingInfoInput(
    EntityInfo EntityInfo,
    string Title,
    string Description,
    int Priority
);
