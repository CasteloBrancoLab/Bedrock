using System.Diagnostics.CodeAnalysis;
using ShopDemo.Auth.Domain.Entities.DPoPKeys;

namespace ShopDemo.Auth.Domain.Validators;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct DPoPProofInfo(
    JwkThumbprint JwkThumbprint,
    string PublicKeyJwk,
    string HttpMethod,
    string HttpUri,
    DateTimeOffset IssuedAt
);
