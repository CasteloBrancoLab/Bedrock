using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace ShopDemo.Auth.Domain.Entities.TokenExchanges.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoTokenExchangeInput(
    EntityInfo EntityInfo,
    Id UserId,
    string SubjectTokenJti,
    string RequestedAudience,
    string IssuedTokenJti,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt
);
