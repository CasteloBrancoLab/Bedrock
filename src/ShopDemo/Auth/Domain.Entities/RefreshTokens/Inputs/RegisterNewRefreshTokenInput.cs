using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewRefreshTokenInput(
    Id UserId,
    TokenHash TokenHash,
    TokenFamily FamilyId,
    DateTimeOffset ExpiresAt
);
