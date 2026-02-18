using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.DPoPKeys.Enums;

namespace ShopDemo.Auth.Domain.Entities.DPoPKeys.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoDPoPKeyInput(
    EntityInfo EntityInfo,
    Id UserId,
    JwkThumbprint JwkThumbprint,
    string PublicKeyJwk,
    DateTimeOffset ExpiresAt,
    DPoPKeyStatus Status,
    DateTimeOffset? RevokedAt
);
