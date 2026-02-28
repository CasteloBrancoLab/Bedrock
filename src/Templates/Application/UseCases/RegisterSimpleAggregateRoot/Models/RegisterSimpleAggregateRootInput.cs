using Bedrock.BuildingBlocks.Core.BirthDates;

namespace Templates.Application.UseCases.RegisterSimpleAggregateRoot.Models;

public sealed record RegisterSimpleAggregateRootInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate
);
