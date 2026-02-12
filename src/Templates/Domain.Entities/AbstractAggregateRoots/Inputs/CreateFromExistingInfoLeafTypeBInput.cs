using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Templates.Domain.Entities.AbstractAggregateRoots.Enums;

namespace Templates.Domain.Entities.AbstractAggregateRoots.Inputs;

public readonly record struct CreateFromExistingInfoLeafTypeBInput(
    EntityInfo EntityInfo,
    string SampleProperty,
    CategoryType CategoryType,
    string LeafProperty
);
