namespace Templates.Domain.Entities.CompositeAggregateRoots.Inputs;

public readonly record struct ChangeCompositeChildEntityTitleInput(
    Guid CompositeChildEntityId,
    string Title
);
