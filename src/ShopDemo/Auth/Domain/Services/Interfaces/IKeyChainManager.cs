using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.KeyChains;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IKeyChainManager
{
    Task<KeyChain?> RotateKeyAsync(
        ExecutionContext executionContext,
        Id userId,
        string clientPublicKeyBase64,
        CancellationToken cancellationToken);

    Task<KeyChain?> ResolveKeyForDecryptionAsync(
        ExecutionContext executionContext,
        Id userId,
        KeyId keyId,
        CancellationToken cancellationToken);

    Task<int> CleanupExpiredAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
