namespace Templates.Domain.Entities.AbstractAggregateRoots.Inputs;

public readonly record struct RegisterNewLeafTypeAInput(
    string SampleProperty,
    string LeafProperty
);
