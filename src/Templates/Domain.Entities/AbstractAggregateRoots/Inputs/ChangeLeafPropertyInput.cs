namespace Templates.Domain.Entities.AbstractAggregateRoots.Inputs;

public readonly record struct ChangeLeafPropertyInput(
    string LeafProperty
);
