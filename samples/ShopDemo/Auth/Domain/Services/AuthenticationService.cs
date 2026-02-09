using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Security.Passwords;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.Auth.Domain.Repositories;

namespace ShopDemo.Auth.Domain.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private const string InvalidCredentialsMessageCode = "AuthenticationService.InvalidCredentials";

    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;

    public AuthenticationService(
        IPasswordHasher passwordHasher,
        IUserRepository userRepository
    )
    {
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<User?> RegisterUserAsync(
        ExecutionContext executionContext,
        string email,
        string password,
        CancellationToken cancellationToken
    )
    {
        bool isValidPassword = PasswordPolicy.ValidatePassword(executionContext, password);

        if (!isValidPassword)
            return null;

        var hashResult = _passwordHasher.HashPassword(executionContext, password);
        var emailAddress = EmailAddress.CreateNew(email);
        var passwordHash = PasswordHash.CreateNew(hashResult.Hash);
        var input = new RegisterNewInput(emailAddress, passwordHash);

        var user = User.RegisterNew(executionContext, input);

        if (user is null)
            return null;

        bool persisted = await _userRepository.RegisterNewAsync(executionContext, user, cancellationToken);

        if (!persisted)
            return null;

        return user;
    }

    public async Task<User?> VerifyCredentialsAsync(
        ExecutionContext executionContext,
        string email,
        string password,
        CancellationToken cancellationToken
    )
    {
        var emailAddress = EmailAddress.CreateNew(email);
        var user = await _userRepository.GetByEmailAsync(executionContext, emailAddress, cancellationToken);

        if (user is null)
        {
            executionContext.AddErrorMessage(code: InvalidCredentialsMessageCode);
            return null;
        }

        byte[] storedHash = user.PasswordHash.Value.ToArray();
        var verificationResult = _passwordHasher.VerifyPassword(executionContext, password, storedHash);

        if (!verificationResult.IsValid)
        {
            executionContext.AddErrorMessage(code: InvalidCredentialsMessageCode);
            return null;
        }

        if (verificationResult.NeedsRehash)
        {
            var newHashResult = _passwordHasher.HashPassword(executionContext, password);
            var newPasswordHash = PasswordHash.CreateNew(newHashResult.Hash);
            var changeInput = new ChangePasswordHashInput(newPasswordHash);
            user = user.ChangePasswordHash(executionContext, changeInput);
        }

        return user;
    }
}
