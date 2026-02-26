using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.UserConsents.Enums;

namespace ShopDemo.Auth.Domain.Entities.UserConsents.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoUserConsentInput(
    EntityInfo EntityInfo,
    Id UserId,
    Id ConsentTermId,
    DateTimeOffset AcceptedAt,
    UserConsentStatus Status,
    DateTimeOffset? RevokedAt,
    string? IpAddress
);
