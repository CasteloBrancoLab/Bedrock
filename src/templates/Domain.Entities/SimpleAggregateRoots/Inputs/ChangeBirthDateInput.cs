using Bedrock.BuildingBlocks.Core.BirthDates;

namespace Templates.Domain.Entities.SimpleAggregateRoots.Inputs;

public readonly record struct ChangeBirthDateInput(
    BirthDate BirthDate
);
