using Bedrock.BuildingBlocks.Application.UseCases;
using Bedrock.BuildingBlocks.Application.UseCases.Models;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Application.UseCases.AuthenticateUser;

public sealed class AuthenticateUserUseCase
    : UseCaseBase<AuthenticateUserInput, AuthenticateUserOutput>, IAuthenticateUserUseCase
{
    private const string AuthenticationFailedMessageCode = "AuthenticateUserUseCase.AuthenticationFailed";

    private readonly IAuthenticationService _authenticationService;

    public AuthenticateUserUseCase(
        ILogger<AuthenticateUserUseCase> logger,
        IAuthenticationService authenticationService
    ) : base(logger)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
    }

    protected override void ConfigureExecutionInternal(UseCaseExecutionOptions options) { }

    protected override async Task<AuthenticateUserOutput?> ExecuteInternalAsync(
        ExecutionContext executionContext,
        AuthenticateUserInput input,
        CancellationToken cancellationToken
    )
    {
        var user = await _authenticationService.VerifyCredentialsAsync(
            executionContext,
            input.Email,
            input.Password,
            cancellationToken);

        if (user is null)
        {
            if (!executionContext.HasErrorMessages)
                executionContext.AddErrorMessage(code: AuthenticationFailedMessageCode);

            return null;
        }

        return new AuthenticateUserOutput(
            user.EntityInfo.Id.Value,
            user.Email.Value ?? string.Empty);
    }
}
