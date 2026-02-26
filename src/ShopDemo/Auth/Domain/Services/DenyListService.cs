using ShopDemo.Auth.Domain.Entities.DenyListEntries;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Enums;
using ShopDemo.Auth.Domain.Entities.DenyListEntries.Inputs;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class DenyListService : IDenyListService
{
    private readonly IDenyListRepository _denyListRepository;

    public DenyListService(
        IDenyListRepository denyListRepository
    )
    {
        _denyListRepository = denyListRepository ?? throw new ArgumentNullException(nameof(denyListRepository));
    }

    public async Task<bool> RevokeTokenAsync(
        ExecutionContext executionContext,
        string jti,
        DateTimeOffset expiresAt,
        string? reason,
        CancellationToken cancellationToken)
    {
        bool alreadyRevoked = await _denyListRepository.ExistsByTypeAndValueAsync(
            executionContext,
            DenyListEntryType.Jti,
            jti,
            cancellationToken);

        if (alreadyRevoked)
            return true;

        var input = new RegisterNewDenyListEntryInput(
            DenyListEntryType.Jti,
            jti,
            expiresAt,
            reason);

        DenyListEntry? entry = DenyListEntry.RegisterNew(executionContext, input);

        if (entry is null)
            return false;

        return await _denyListRepository.RegisterNewAsync(executionContext, entry, cancellationToken);
    }

    public async Task<bool> RevokeUserAsync(
        ExecutionContext executionContext,
        string userId,
        DateTimeOffset expiresAt,
        string? reason,
        CancellationToken cancellationToken)
    {
        bool alreadyRevoked = await _denyListRepository.ExistsByTypeAndValueAsync(
            executionContext,
            DenyListEntryType.UserId,
            userId,
            cancellationToken);

        if (alreadyRevoked)
            return true;

        var input = new RegisterNewDenyListEntryInput(
            DenyListEntryType.UserId,
            userId,
            expiresAt,
            reason);

        DenyListEntry? entry = DenyListEntry.RegisterNew(executionContext, input);

        if (entry is null)
            return false;

        return await _denyListRepository.RegisterNewAsync(executionContext, entry, cancellationToken);
    }

    public Task<bool> IsTokenRevokedAsync(
        ExecutionContext executionContext,
        string jti,
        CancellationToken cancellationToken)
    {
        return _denyListRepository.ExistsByTypeAndValueAsync(
            executionContext,
            DenyListEntryType.Jti,
            jti,
            cancellationToken);
    }

    public Task<bool> IsUserRevokedAsync(
        ExecutionContext executionContext,
        string userId,
        CancellationToken cancellationToken)
    {
        return _denyListRepository.ExistsByTypeAndValueAsync(
            executionContext,
            DenyListEntryType.UserId,
            userId,
            cancellationToken);
    }

    public Task<int> CleanupExpiredAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        return _denyListRepository.DeleteExpiredAsync(
            executionContext,
            executionContext.Timestamp,
            cancellationToken);
    }
}
