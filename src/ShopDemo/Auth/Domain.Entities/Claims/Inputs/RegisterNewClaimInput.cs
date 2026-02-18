using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Domain.Entities.Claims.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewClaimInput(
    string Name,
    string? Description
);
