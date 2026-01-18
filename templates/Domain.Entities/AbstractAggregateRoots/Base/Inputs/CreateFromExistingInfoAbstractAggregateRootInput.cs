using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace Templates.Domain.Entities.AbstractAggregateRoots.Base.Inputs;

public readonly record struct CreateFromExistingInfoAbstractAggregateRootInput(
    EntityInfo EntityInfo,
    string SampleProperty
);
