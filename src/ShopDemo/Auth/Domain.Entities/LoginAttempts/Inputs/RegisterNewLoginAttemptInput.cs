using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Domain.Entities.LoginAttempts.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewLoginAttemptInput(
    string Username,
    string? IpAddress,
    bool IsSuccessful,
    string? FailureReason
);
