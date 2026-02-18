using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Entities.MfaSetups.Interfaces;

public interface IMfaSetup
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    string EncryptedSharedSecret { get; }
    bool IsEnabled { get; }
    DateTimeOffset? EnabledAt { get; }
}
