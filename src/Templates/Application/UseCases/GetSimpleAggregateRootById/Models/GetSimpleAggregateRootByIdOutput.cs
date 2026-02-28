namespace Templates.Application.UseCases.GetSimpleAggregateRootById.Models;

public sealed record GetSimpleAggregateRootByIdOutput(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    DateTimeOffset BirthDate
);
