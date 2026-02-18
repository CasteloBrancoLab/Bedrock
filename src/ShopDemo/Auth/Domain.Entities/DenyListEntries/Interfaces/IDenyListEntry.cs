using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;

namespace ShopDemo.Auth.Domain.Entities.DenyListEntries.Interfaces;

public interface IDenyListEntry
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    DenyListEntryType Type { get; }
    string Value { get; }
    DateTimeOffset ExpiresAt { get; }
    string? Reason { get; }
}
