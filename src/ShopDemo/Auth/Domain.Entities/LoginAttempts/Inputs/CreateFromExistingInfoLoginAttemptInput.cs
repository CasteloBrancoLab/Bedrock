using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace ShopDemo.Auth.Domain.Entities.LoginAttempts.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoLoginAttemptInput(
    EntityInfo EntityInfo,
    string Username,
    string? IpAddress,
    DateTimeOffset AttemptedAt,
    bool IsSuccessful,
    string? FailureReason
);
