using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct TokenExchangeModel(
    Guid Id,
    Guid TenantCode,
    Guid UserId,
    string SubjectTokenJti,
    string RequestedAudience,
    string IssuedTokenJti,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt,
    string CreatedBy
);
