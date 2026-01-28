using Templates.Domain.Entities.AbstractAggregateRoots.Enums;

namespace Templates.Domain.Entities.AbstractAggregateRoots.Base.Inputs;

public readonly record struct RegisterNewAbstractAggregateRootInput(
    string SampleProperty,
    CategoryType CategoryType
);
