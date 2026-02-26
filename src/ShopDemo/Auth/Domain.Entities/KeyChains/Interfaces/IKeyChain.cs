using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;

namespace ShopDemo.Auth.Domain.Entities.KeyChains.Interfaces;

public interface IKeyChain
    : Bedrock.BuildingBlocks.Domain.Entities.Interfaces.IAggregateRoot
{
    Id UserId { get; }
    KeyId KeyId { get; }
    string PublicKey { get; }
    string EncryptedSharedSecret { get; }
    KeyChainStatus Status { get; }
    DateTimeOffset ExpiresAt { get; }
}
