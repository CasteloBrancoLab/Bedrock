using ShopDemo.Auth.Domain.Entities.LoginAttempts;
using ShopDemo.Auth.Domain.Entities.LoginAttempts.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class BruteForceProtectionService : IBruteForceProtectionService
{
    private const int DefaultMaxFailedAttempts = 5;
    private static readonly TimeSpan DefaultLockoutDuration = TimeSpan.FromMinutes(15);

    private readonly ILoginAttemptRepository _loginAttemptRepository;

    public BruteForceProtectionService(
        ILoginAttemptRepository loginAttemptRepository)
    {
        ArgumentNullException.ThrowIfNull(loginAttemptRepository);

        _loginAttemptRepository = loginAttemptRepository;
    }

    public async Task<LoginAttempt?> RecordLoginAttemptAsync(
        ExecutionContext executionContext,
        string username,
        string? ipAddress,
        bool isSuccessful,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        LoginAttempt? loginAttempt = LoginAttempt.RegisterNew(
            executionContext,
            new RegisterNewLoginAttemptInput(
                username,
                ipAddress,
                isSuccessful,
                failureReason));

        if (loginAttempt is null)
            return null;

        bool registered = await _loginAttemptRepository.RegisterNewAsync(
            executionContext,
            loginAttempt,
            cancellationToken);

        if (!registered)
            return null;

        return loginAttempt;
    }

    public async Task<bool> IsLockedOutAsync(
        ExecutionContext executionContext,
        string username,
        CancellationToken cancellationToken)
    {
        DateTimeOffset since = executionContext.Timestamp - DefaultLockoutDuration;

        IReadOnlyList<LoginAttempt> recentAttempts = await _loginAttemptRepository.GetRecentByUsernameAsync(
            executionContext,
            username,
            since,
            cancellationToken);

        int failedCount = recentAttempts.Count(a => !a.IsSuccessful);

        return failedCount >= DefaultMaxFailedAttempts;
    }
}
