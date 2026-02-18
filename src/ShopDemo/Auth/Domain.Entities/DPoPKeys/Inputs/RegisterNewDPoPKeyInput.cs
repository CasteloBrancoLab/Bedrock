using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.DPoPKeys.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewDPoPKeyInput(
    Id UserId,
    JwkThumbprint JwkThumbprint,
    string PublicKeyJwk,
    DateTimeOffset ExpiresAt
);
