namespace Templates.Application.UseCases.RegisterSimpleAggregateRoot.Models;

public sealed record RegisterSimpleAggregateRootOutput(
    Guid Id,
    string FullName
);
