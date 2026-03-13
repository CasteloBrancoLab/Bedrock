using Bedrock.BuildingBlocks.Application.UseCases;
using Bedrock.BuildingBlocks.Application.UseCases.Models;
using Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Application.Factories.Messages.Events;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;
using ShopDemo.Auth.Domain.Services.Interfaces;
using ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;

namespace ShopDemo.Auth.Application.UseCases.AuthenticateUser;

public sealed class AuthenticateUserUseCase
    : UseCaseBase<AuthenticateUserInput, AuthenticateUserOutput>, IAuthenticateUserUseCase
{
    private const string AuthenticationFailedMessageCode = "AuthenticateUserUseCase.AuthenticationFailed";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthOutboxWriter _outboxWriter;
    private readonly TimeProvider _timeProvider;

    public AuthenticateUserUseCase(
        ILogger<AuthenticateUserUseCase> logger,
        IUnitOfWork unitOfWork,
        IAuthenticationService authenticationService,
        IAuthOutboxWriter outboxWriter,
        TimeProvider timeProvider
    ) : base(logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _outboxWriter = outboxWriter ?? throw new ArgumentNullException(nameof(outboxWriter));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    protected override void ConfigureExecutionInternal(UseCaseExecutionOptions options)
    {
        options.WithTransaction(_unitOfWork);
    }

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

        var @event = UserAuthenticatedEventFactory.Create(
            executionContext, _timeProvider, input.Email, user);

        await _outboxWriter.EnqueueAsync(@event, cancellationToken);

        return new AuthenticateUserOutput(
            user.EntityInfo.Id.Value,
            user.Email.Value ?? string.Empty);
    }
}
