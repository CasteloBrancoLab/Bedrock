namespace Templates.Domain.Entities.CompositeAggregateRoots.Inputs;

public readonly record struct ChildRegisterNewInput(
    string Title,
    string Description,
    int Priority
);
