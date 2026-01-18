using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace Templates.Domain.Entities.SimpleAggregateRoots.Inputs;

public readonly record struct CreateFromExistingInfoInput(
    EntityInfo EntityInfo,
    string FirstName,
    string LastName,
    string FullName,
    BirthDate BirthDate
);