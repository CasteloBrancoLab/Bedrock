using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;

namespace ShopDemo.Auth.Domain.Entities.SigningKeys.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoSigningKeyInput(
    EntityInfo EntityInfo,
    Kid Kid,
    string Algorithm,
    string PublicKey,
    string EncryptedPrivateKey,
    SigningKeyStatus Status,
    DateTimeOffset? RotatedAt,
    DateTimeOffset ExpiresAt
);
