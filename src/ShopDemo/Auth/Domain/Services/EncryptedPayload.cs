using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Domain.Services;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct EncryptedPayload(
    byte[] Ciphertext,
    byte[] Nonce,
    byte[] Tag
);
