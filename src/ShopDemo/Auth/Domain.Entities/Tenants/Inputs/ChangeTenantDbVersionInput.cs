using System.Diagnostics.CodeAnalysis;

namespace ShopDemo.Auth.Domain.Entities.Tenants.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct ChangeTenantDbVersionInput(
    string DbVersion
);
