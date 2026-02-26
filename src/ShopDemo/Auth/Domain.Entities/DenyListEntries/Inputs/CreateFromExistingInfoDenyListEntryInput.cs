using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;

namespace ShopDemo.Auth.Domain.Entities.DenyListEntries.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoDenyListEntryInput(
    EntityInfo EntityInfo,
    DenyListEntryType Type,
    string Value,
    DateTimeOffset ExpiresAt,
    string? Reason
);
