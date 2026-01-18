namespace Templates.Domain.Entities.AssociatedAggregateRoots.Inputs;

public readonly record struct PrimaryChangeReferencedAggregateRootInput(
    ReferencedAggregateRoot? ReferencedAggregateRoot
);
