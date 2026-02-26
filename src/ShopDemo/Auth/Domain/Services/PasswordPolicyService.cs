using Bedrock.BuildingBlocks.Core.Ids;
using ShopDemo.Auth.Domain.Entities.PasswordHistories;
using ShopDemo.Auth.Domain.Entities.PasswordHistories.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class PasswordPolicyService : IPasswordPolicyService
{
    private const int MinPasswordLength = 12;
    private const int MaxPasswordLength = 128;
    private const int MaxHistoryCount = 5;

    private readonly IPasswordHistoryRepository _passwordHistoryRepository;
    private readonly IPasswordBreachChecker _passwordBreachChecker;

    public PasswordPolicyService(
        IPasswordHistoryRepository passwordHistoryRepository,
        IPasswordBreachChecker passwordBreachChecker)
    {
        ArgumentNullException.ThrowIfNull(passwordHistoryRepository);
        ArgumentNullException.ThrowIfNull(passwordBreachChecker);

        _passwordHistoryRepository = passwordHistoryRepository;
        _passwordBreachChecker = passwordBreachChecker;
    }

    public async Task<bool> ValidatePasswordAsync(
        ExecutionContext executionContext,
        string password,
        Id? userId,
        CancellationToken cancellationToken)
    {
        bool isValid = true;

        if (password.Length < MinPasswordLength)
        {
            executionContext.AddErrorMessage(code: "PasswordPolicy.TooShort");
            isValid = false;
        }

        if (password.Length > MaxPasswordLength)
        {
            executionContext.AddErrorMessage(code: "PasswordPolicy.TooLong");
            isValid = false;
        }

        if (!isValid)
            return false;

        bool isBreached = await _passwordBreachChecker.IsBreachedAsync(
            password,
            cancellationToken);

        if (isBreached)
        {
            executionContext.AddErrorMessage(code: "PasswordPolicy.Breached");
            return false;
        }

        return true;
    }

    public async Task<bool> RecordPasswordChangeAsync(
        ExecutionContext executionContext,
        Id userId,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        PasswordHistory? passwordHistory = PasswordHistory.RegisterNew(
            executionContext,
            new RegisterNewPasswordHistoryInput(
                userId,
                passwordHash));

        if (passwordHistory is null)
            return false;

        bool registered = await _passwordHistoryRepository.RegisterNewAsync(
            executionContext,
            passwordHistory,
            cancellationToken);

        return registered;
    }
}
