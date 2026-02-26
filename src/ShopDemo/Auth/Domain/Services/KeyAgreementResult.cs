using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Domain.Services;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct KeyAgreementResult(
    string ServerPublicKeyBase64,
    byte[] SharedSecret
);
