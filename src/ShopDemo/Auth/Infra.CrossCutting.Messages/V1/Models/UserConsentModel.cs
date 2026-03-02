using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct UserConsentModel(
    Guid Id,
    Guid TenantCode,
    Guid UserId,
    Guid ConsentTermId,
    DateTimeOffset AcceptedAt,
    string Status,
    DateTimeOffset? RevokedAt,
    string? IpAddress,
    DateTimeOffset CreatedAt,
    string CreatedBy
);
