using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct AccountLockoutOutputModel(
    string Username,
    string? IpAddress,
    int AttemptCount,
    DateTimeOffset? LockedUntil
);
