using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;

namespace ShopDemo.Auth.Domain.Entities.RefreshTokens.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoRefreshTokenInput(
    EntityInfo EntityInfo,
    Id UserId,
    TokenHash TokenHash,
    TokenFamily FamilyId,
    DateTimeOffset ExpiresAt,
    RefreshTokenStatus Status,
    DateTimeOffset? RevokedAt,
    Id? ReplacedByTokenId
);
