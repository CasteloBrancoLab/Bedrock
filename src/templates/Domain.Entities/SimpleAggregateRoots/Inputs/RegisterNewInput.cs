using Bedrock.BuildingBlocks.Core.BirthDates;

namespace Templates.Domain.Entities.SimpleAggregateRoots.Inputs;

public readonly record struct RegisterNewInput(
    string FirstName,
    string LastName,
    BirthDate BirthDate
);