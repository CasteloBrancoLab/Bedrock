using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.KeyChains;
using ShopDemo.Auth.Domain.Entities.KeyChains.Enums;
using ShopDemo.Auth.Domain.Entities.KeyChains.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class KeyChainManager : IKeyChainManager
{
    private const int DefaultKeyTtlDays = 30;

    private readonly IKeyChainRepository _keyChainRepository;
    private readonly IKeyAgreementService _keyAgreementService;

    public KeyChainManager(
        IKeyChainRepository keyChainRepository,
        IKeyAgreementService keyAgreementService
    )
    {
        _keyChainRepository = keyChainRepository ?? throw new ArgumentNullException(nameof(keyChainRepository));
        _keyAgreementService = keyAgreementService ?? throw new ArgumentNullException(nameof(keyAgreementService));
    }

    public async Task<KeyChain?> RotateKeyAsync(
        ExecutionContext executionContext,
        Id userId,
        string clientPublicKeyBase64,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<KeyChain> existingKeys = await _keyChainRepository.GetByUserIdAsync(
            executionContext,
            userId,
            cancellationToken);

        KeyChain? activeKey = existingKeys.FirstOrDefault(
            static k => k.Status == KeyChainStatus.Active);

        if (activeKey is not null)
        {
            KeyChain? deactivated = activeKey.Deactivate(
                executionContext,
                new DeactivateKeyChainInput());

            if (deactivated is not null)
            {
                await _keyChainRepository.UpdateAsync(
                    executionContext,
                    deactivated,
                    cancellationToken);
            }
        }

        KeyAgreementResult agreementResult = _keyAgreementService.NegotiateKey(clientPublicKeyBase64);

        int nextVersion = existingKeys.Count + 1;
        KeyId newKeyId = KeyId.CreateNew($"v{nextVersion}");

        var input = new RegisterNewKeyChainInput(
            userId,
            newKeyId,
            agreementResult.ServerPublicKeyBase64,
            Convert.ToBase64String(agreementResult.SharedSecret),
            executionContext.Timestamp.AddDays(DefaultKeyTtlDays));

        KeyChain? newKey = KeyChain.RegisterNew(executionContext, input);

        if (newKey is null)
            return null;

        bool persisted = await _keyChainRepository.RegisterNewAsync(
            executionContext,
            newKey,
            cancellationToken);

        if (!persisted)
            return null;

        return newKey;
    }

    public Task<KeyChain?> ResolveKeyForDecryptionAsync(
        ExecutionContext executionContext,
        Id userId,
        KeyId keyId,
        CancellationToken cancellationToken)
    {
        return _keyChainRepository.GetByUserIdAndKeyIdAsync(
            executionContext,
            userId,
            keyId,
            cancellationToken);
    }

    public Task<int> CleanupExpiredAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        return _keyChainRepository.DeleteExpiredAsync(
            executionContext,
            executionContext.Timestamp,
            cancellationToken);
    }
}
