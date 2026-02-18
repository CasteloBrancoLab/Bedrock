using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.ApiKeys.Enums;

namespace ShopDemo.Auth.Domain.Entities.ApiKeys.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoApiKeyInput(
    EntityInfo EntityInfo,
    Id ServiceClientId,
    string KeyPrefix,
    string KeyHash,
    ApiKeyStatus Status,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? LastUsedAt,
    DateTimeOffset? RevokedAt
);
