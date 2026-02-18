namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IDenyListService
{
    Task<bool> RevokeTokenAsync(
        ExecutionContext executionContext,
        string jti,
        DateTimeOffset expiresAt,
        string? reason,
        CancellationToken cancellationToken);

    Task<bool> RevokeUserAsync(
        ExecutionContext executionContext,
        string userId,
        DateTimeOffset expiresAt,
        string? reason,
        CancellationToken cancellationToken);

    Task<bool> IsTokenRevokedAsync(
        ExecutionContext executionContext,
        string jti,
        CancellationToken cancellationToken);

    Task<bool> IsUserRevokedAsync(
        ExecutionContext executionContext,
        string userId,
        CancellationToken cancellationToken);

    Task<int> CleanupExpiredAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken);
}
