using ShopDemo.Auth.Domain.Entities.Tenants.Enums;

namespace ShopDemo.Auth.Domain.Entities.Tenants.Interfaces;

public interface ITenant
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    string Name { get; }
    string Domain { get; }
    string SchemaName { get; }
    TenantStatus Status { get; }
    TenantTier Tier { get; }
    string? DbVersion { get; }
}
