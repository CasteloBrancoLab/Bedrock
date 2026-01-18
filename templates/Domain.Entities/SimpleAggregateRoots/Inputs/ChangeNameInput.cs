namespace Templates.Domain.Entities.SimpleAggregateRoots.Inputs;

public readonly record struct ChangeNameInput(
    string FirstName,
    string LastName
);
