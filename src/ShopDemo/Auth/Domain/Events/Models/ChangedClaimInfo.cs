using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.Claims;

namespace ShopDemo.Auth.Domain.Events.Models;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct ChangedClaimInfo(
    Id ClaimId,
    ClaimValue OldValue,
    ClaimValue NewValue
);
