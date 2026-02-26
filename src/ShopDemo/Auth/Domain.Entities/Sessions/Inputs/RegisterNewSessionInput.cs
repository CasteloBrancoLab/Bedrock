using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.Sessions.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewSessionInput(
    Id UserId,
    Id RefreshTokenId,
    string? DeviceInfo,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset ExpiresAt
);
