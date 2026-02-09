using ShopDemo.Auth.Domain.Entities.Users;

namespace ShopDemo.Auth.Domain.Services;

public interface IAuthenticationService
{
    Task<User?> RegisterUserAsync(
        ExecutionContext executionContext,
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<User?> VerifyCredentialsAsync(
        ExecutionContext executionContext,
        string email,
        string password,
        CancellationToken cancellationToken);
}
