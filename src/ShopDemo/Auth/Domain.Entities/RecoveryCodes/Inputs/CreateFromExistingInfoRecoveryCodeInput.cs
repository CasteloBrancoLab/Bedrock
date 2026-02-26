using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace ShopDemo.Auth.Domain.Entities.RecoveryCodes.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoRecoveryCodeInput(
    EntityInfo EntityInfo,
    Id UserId,
    string CodeHash,
    bool IsUsed,
    DateTimeOffset? UsedAt
);
