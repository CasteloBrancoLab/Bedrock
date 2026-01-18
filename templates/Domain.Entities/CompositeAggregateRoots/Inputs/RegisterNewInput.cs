namespace Templates.Domain.Entities.CompositeAggregateRoots.Inputs;

public readonly record struct RegisterNewInput(
    string Name,
    string Code,
    IEnumerable<ChildRegisterNewInput> ChildRegisterNewInputCollection
);
