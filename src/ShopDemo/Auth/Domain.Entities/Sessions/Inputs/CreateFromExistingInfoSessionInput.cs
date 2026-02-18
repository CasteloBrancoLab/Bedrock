using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.Sessions.Enums;

namespace ShopDemo.Auth.Domain.Entities.Sessions.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoSessionInput(
    EntityInfo EntityInfo,
    Id UserId,
    Id RefreshTokenId,
    string? DeviceInfo,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset ExpiresAt,
    SessionStatus Status,
    DateTimeOffset LastActivityAt,
    DateTimeOffset? RevokedAt
);
