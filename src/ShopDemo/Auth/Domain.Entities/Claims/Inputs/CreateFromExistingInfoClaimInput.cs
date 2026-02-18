using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace ShopDemo.Auth.Domain.Entities.Claims.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoClaimInput(
    EntityInfo EntityInfo,
    string Name,
    string? Description
);
