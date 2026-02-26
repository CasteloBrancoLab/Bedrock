using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;

namespace ShopDemo.Auth.Domain.Entities.KeyChains.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoKeyChainInput(
    EntityInfo EntityInfo,
    Id UserId,
    KeyId KeyId,
    string PublicKey,
    string EncryptedSharedSecret,
    KeyChainStatus Status,
    DateTimeOffset ExpiresAt
);
