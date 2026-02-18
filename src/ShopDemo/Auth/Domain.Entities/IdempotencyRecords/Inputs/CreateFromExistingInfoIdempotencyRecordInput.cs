using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Domain.Entities.Models;

namespace ShopDemo.Auth.Domain.Entities.IdempotencyRecords.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoIdempotencyRecordInput(
    EntityInfo EntityInfo,
    string IdempotencyKey,
    string RequestHash,
    string? ResponseBody,
    int StatusCode,
    DateTimeOffset ExpiresAt
);
