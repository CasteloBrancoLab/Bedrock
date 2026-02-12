using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace Templates.Domain.Entities.AssociatedAggregateRoots.Inputs;

public readonly record struct CreateFromExistingInfoInput(
    EntityInfo EntityInfo,
    string SampleName
);
