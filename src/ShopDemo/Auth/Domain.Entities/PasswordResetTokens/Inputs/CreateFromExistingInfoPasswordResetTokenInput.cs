using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace ShopDemo.Auth.Domain.Entities.PasswordResetTokens.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoPasswordResetTokenInput(
    EntityInfo EntityInfo,
    Id UserId,
    string TokenHash,
    DateTimeOffset ExpiresAt,
    bool IsUsed,
    DateTimeOffset? UsedAt
);
