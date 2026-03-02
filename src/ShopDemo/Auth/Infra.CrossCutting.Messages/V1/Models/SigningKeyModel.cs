using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct SigningKeyModel(
    Guid Id,
    Guid TenantCode,
    string Kid,
    string Algorithm,
    string PublicKey,
    string Status,
    DateTimeOffset? RotatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt,
    string CreatedBy
);
