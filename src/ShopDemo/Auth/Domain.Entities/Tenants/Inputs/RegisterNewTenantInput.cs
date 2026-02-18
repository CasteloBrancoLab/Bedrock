using System.Diagnostics.CodeAnalysis;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;

namespace ShopDemo.Auth.Domain.Entities.Tenants.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewTenantInput(
    string Name,
    string Domain,
    string SchemaName,
    TenantTier Tier
);
