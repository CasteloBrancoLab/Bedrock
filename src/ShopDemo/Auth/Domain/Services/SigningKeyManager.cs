using System.Security.Cryptography;
using ShopDemo.Auth.Domain.Entities.SigningKeys;
using ShopDemo.Auth.Domain.Entities.SigningKeys.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class SigningKeyManager : ISigningKeyManager
{
    private const int DefaultKeyTtlDays = 90;
    private const string DefaultAlgorithm = "ES256";

    private readonly ISigningKeyRepository _signingKeyRepository;

    public SigningKeyManager(
        ISigningKeyRepository signingKeyRepository
    )
    {
        _signingKeyRepository = signingKeyRepository ?? throw new ArgumentNullException(nameof(signingKeyRepository));
    }

    public async Task<SigningKey?> RotateKeyAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        SigningKey? currentKey = await _signingKeyRepository.GetActiveAsync(
            executionContext,
            cancellationToken);

        if (currentKey is not null)
        {
            SigningKey? rotated = currentKey.Rotate(
                executionContext,
                new RotateSigningKeyInput());

            if (rotated is not null)
            {
                await _signingKeyRepository.UpdateAsync(
                    executionContext,
                    rotated,
                    cancellationToken);
            }
        }

        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        string publicKey = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo());
        string privateKey = Convert.ToBase64String(ecdsa.ExportPkcs8PrivateKey());

        Kid kid = Kid.CreateNew($"kid-{Guid.NewGuid():N}");

        var input = new RegisterNewSigningKeyInput(
            kid,
            DefaultAlgorithm,
            publicKey,
            privateKey,
            executionContext.Timestamp.AddDays(DefaultKeyTtlDays));

        // RegisterNew sempre sucede com inputs gerados internamente (ECDsa, Kid valido, ES256)
        SigningKey newKey = SigningKey.RegisterNew(executionContext, input)!;

        bool persisted = await _signingKeyRepository.RegisterNewAsync(
            executionContext,
            newKey,
            cancellationToken);

        if (!persisted)
            return null;

        return newKey;
    }

    public Task<SigningKey?> GetCurrentKeyAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        return _signingKeyRepository.GetActiveAsync(
            executionContext,
            cancellationToken);
    }

    public Task<SigningKey?> GetKeyByKidAsync(
        ExecutionContext executionContext,
        Kid kid,
        CancellationToken cancellationToken)
    {
        return _signingKeyRepository.GetByKidAsync(
            executionContext,
            kid,
            cancellationToken);
    }
}
