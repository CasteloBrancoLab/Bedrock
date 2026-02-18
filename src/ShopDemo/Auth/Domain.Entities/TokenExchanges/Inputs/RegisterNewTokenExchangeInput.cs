using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.TokenExchanges.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewTokenExchangeInput(
    Id UserId,
    string SubjectTokenJti,
    string RequestedAudience,
    string IssuedTokenJti,
    DateTimeOffset ExpiresAt
);
