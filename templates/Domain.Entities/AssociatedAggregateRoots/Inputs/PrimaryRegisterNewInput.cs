namespace Templates.Domain.Entities.AssociatedAggregateRoots.Inputs;

public readonly record struct PrimaryRegisterNewInput(
    int Quantity,
    ReferencedAggregateRoot? ReferencedAggregateRoot
);
