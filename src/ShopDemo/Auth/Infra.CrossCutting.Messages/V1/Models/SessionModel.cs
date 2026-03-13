using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct SessionModel(
    Guid Id,
    Guid TenantCode,
    Guid UserId,
    Guid RefreshTokenId,
    string? DeviceInfo,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset ExpiresAt,
    string Status,
    DateTimeOffset LastActivityAt,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt,
    string CreatedBy
);
