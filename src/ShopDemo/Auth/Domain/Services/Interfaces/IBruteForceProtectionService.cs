using ShopDemo.Auth.Domain.Entities.LoginAttempts;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IBruteForceProtectionService
{
    Task<LoginAttempt?> RecordLoginAttemptAsync(
        ExecutionContext executionContext,
        string username,
        string? ipAddress,
        bool isSuccessful,
        string? failureReason,
        CancellationToken cancellationToken);

    Task<bool> IsLockedOutAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken);
}
