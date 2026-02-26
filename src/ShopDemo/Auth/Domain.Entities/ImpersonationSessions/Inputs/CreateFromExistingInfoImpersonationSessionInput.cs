using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Enums;

namespace ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct CreateFromExistingInfoImpersonationSessionInput(
    EntityInfo EntityInfo,
    Id OperatorUserId,
    Id TargetUserId,
    DateTimeOffset ExpiresAt,
    ImpersonationSessionStatus Status,
    DateTimeOffset? EndedAt
);
