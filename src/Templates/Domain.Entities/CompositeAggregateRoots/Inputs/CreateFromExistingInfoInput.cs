using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace Templates.Domain.Entities.CompositeAggregateRoots.Inputs;

public readonly record struct CreateFromExistingInfoInput(
    EntityInfo EntityInfo,
    string Name,
    string Code,
    IEnumerable<CompositeChildEntity> CompositeChildEntities
);
