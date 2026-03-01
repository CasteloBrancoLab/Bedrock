using Bedrock.BuildingBlocks.Application.UseCases;
using Bedrock.BuildingBlocks.Application.UseCases.Models;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces;
using Microsoft.Extensions.Logging;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Models;
using ShopDemo.Auth.Domain.Services.Interfaces;
using ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Application.UseCases.RegisterUser;

public sealed class RegisterUserUseCase
    : UseCaseBase<RegisterUserInput, RegisterUserOutput>, IRegisterUserUseCase
{
    private const string RegistrationFailedMessageCode = "RegisterUserUseCase.RegistrationFailed";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthOutboxWriter _outboxWriter;
    private readonly TimeProvider _timeProvider;

    public RegisterUserUseCase(
        ILogger<RegisterUserUseCase> logger,
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
        options.UnitOfWork = _unitOfWork;
    }

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

        var @event = new UserRegisteredEvent(
            Metadata: new MessageMetadata(
                MessageId: Id.GenerateNewId(_timeProvider).Value,
                Timestamp: _timeProvider.GetUtcNow(),
                SchemaName: string.Empty,
                CorrelationId: executionContext.CorrelationId,
                TenantCode: executionContext.TenantInfo.Code,
                ExecutionUser: executionContext.ExecutionUser,
                ExecutionOrigin: executionContext.ExecutionOrigin,
                BusinessOperationCode: executionContext.BusinessOperationCode),
            Input: new RegisterUserInputModel(
                Email: input.Email),
            NewState: new UserModel(
                Id: user.EntityInfo.Id.Value,
                TenantCode: executionContext.TenantInfo.Code,
                Email: user.Email.Value ?? string.Empty,
                CreatedAt: user.EntityInfo.EntityChangeInfo.CreatedAt,
                CreatedBy: user.EntityInfo.EntityChangeInfo.CreatedBy));

        await _outboxWriter.EnqueueAsync(@event, cancellationToken);

        return new RegisterUserOutput(
            user.EntityInfo.Id.Value,
            user.Email.Value ?? string.Empty);
    }
}
