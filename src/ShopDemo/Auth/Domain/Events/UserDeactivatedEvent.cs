using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Events;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct UserDeactivatedEvent(
    Id UserId,
    string? Reason,
    int RevokedRefreshTokenCount,
    int RevokedServiceClientCount,
    int RevokedApiKeyCount
);
