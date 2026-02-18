using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.KeyChains.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewKeyChainInput(
    Id UserId,
    KeyId KeyId,
    string PublicKey,
    string EncryptedSharedSecret,
    DateTimeOffset ExpiresAt
);
