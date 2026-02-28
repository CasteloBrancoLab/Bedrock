using Bedrock.BuildingBlocks.Application.UseCases;
using Bedrock.BuildingBlocks.Application.UseCases.Models;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Models;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Application.UseCases.RegisterUser;

public sealed class RegisterUserUseCase
    : UseCaseBase<RegisterUserInput, RegisterUserOutput>, IRegisterUserUseCase
{
    private const string RegistrationFailedMessageCode = "RegisterUserUseCase.RegistrationFailed";

    private readonly IAuthenticationService _authenticationService;

    public RegisterUserUseCase(
        ILogger<RegisterUserUseCase> logger,
        IAuthenticationService authenticationService
    ) : base(logger)
    {
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
    }

    protected override void ConfigureExecutionInternal(UseCaseExecutionOptions options) { }

    protected override async Task<RegisterUserOutput?> ExecuteInternalAsync(
        ExecutionContext executionContext,
        RegisterUserInput input,
        CancellationToken cancellationToken
    )
    {
        var user = await _authenticationService.RegisterUserAsync(
            executionContext,
            input.Email,
            input.Password,
            cancellationToken);

        if (user is null)
        {
            if (!executionContext.HasErrorMessages)
                executionContext.AddErrorMessage(code: RegistrationFailedMessageCode);

            return null;
        }

        return new RegisterUserOutput(
            user.EntityInfo.Id.Value,
            user.Email.Value ?? string.Empty);
    }
}
