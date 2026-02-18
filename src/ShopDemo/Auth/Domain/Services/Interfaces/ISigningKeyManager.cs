using ShopDemo.Auth.Domain.Entities.SigningKeys;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface ISigningKeyManager
{
    Task<SigningKey?> RotateKeyAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);

    Task<SigningKey?> GetCurrentKeyAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);

    Task<SigningKey?> GetKeyByKidAsync(
        ExecutionContext executionContext,
        Kid kid,
        CancellationToken cancellationToken);
}
