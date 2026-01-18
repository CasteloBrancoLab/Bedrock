namespace Templates.Domain.Entities.AbstractAggregateRoots.Inputs;

public readonly record struct RegisterNewLeafTypeBInput(
    string SampleProperty,
    string LeafProperty
);
