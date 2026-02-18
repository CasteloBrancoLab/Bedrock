using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.RecoveryCodes.Interfaces;

public interface IRecoveryCode
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    string CodeHash { get; }
    bool IsUsed { get; }
    DateTimeOffset? UsedAt { get; }
}
