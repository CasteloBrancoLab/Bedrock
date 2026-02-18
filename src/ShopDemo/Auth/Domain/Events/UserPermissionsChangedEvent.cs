using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Events.Models;

namespace ShopDemo.Auth.Domain.Events;

[ExcludeFromCodeCoverage(Justification = "Record class com propriedade de colecao — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public sealed record UserPermissionsChangedEvent(
    Id UserId,
    IReadOnlyList<ChangedClaimInfo> ChangedClaims
);
