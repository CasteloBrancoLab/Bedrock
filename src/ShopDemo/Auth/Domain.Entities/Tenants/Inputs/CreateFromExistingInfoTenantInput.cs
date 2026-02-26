using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.Tenants.Enums;

namespace ShopDemo.Auth.Domain.Entities.Tenants.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoTenantInput(
    EntityInfo EntityInfo,
    string Name,
    string Domain,
    string SchemaName,
    TenantStatus Status,
    TenantTier Tier,
    string? DbVersion
);
