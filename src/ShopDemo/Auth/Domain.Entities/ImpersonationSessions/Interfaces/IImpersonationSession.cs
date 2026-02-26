using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Enums;

namespace ShopDemo.Auth.Domain.Entities.ImpersonationSessions.Interfaces;

public interface IImpersonationSession
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id OperatorUserId { get; }
    Id TargetUserId { get; }
    DateTimeOffset ExpiresAt { get; }
    ImpersonationSessionStatus Status { get; }
    DateTimeOffset? EndedAt { get; }
}
