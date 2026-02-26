using Bedrock.BuildingBlocks.Core.Ids;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IPasswordPolicyService
{
    Task<bool> ValidatePasswordAsync(
        ExecutionContext executionContext,
        string password,
        Id? userId,
        CancellationToken cancellationToken);

    Task<bool> RecordPasswordChangeAsync(
        ExecutionContext executionContext,
        Id userId,
        string passwordHash,
        CancellationToken cancellationToken);
}
