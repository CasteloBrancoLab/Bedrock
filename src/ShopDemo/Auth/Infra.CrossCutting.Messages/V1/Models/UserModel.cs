using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct UserModel(
    Guid Id,
    Guid TenantCode,
    string Username,
    string Email,
    string Status,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset? LastChangedAt,
    string? LastChangedBy
);
