using Bedrock.BuildingBlocks.Application.UseCases;
using Bedrock.BuildingBlocks.Application.UseCases.Models;
using Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces;
using Microsoft.Extensions.Logging;
using Templates.Application.UseCases.RegisterSimpleAggregateRoot.Interfaces;
using Templates.Application.UseCases.RegisterSimpleAggregateRoot.Models;
using Templates.Domain.Entities.SimpleAggregateRoots;
using Templates.Domain.Entities.SimpleAggregateRoots.Inputs;
using Templates.Domain.Repositories.Interfaces;

namespace Templates.Application.UseCases.RegisterSimpleAggregateRoot;

/// <summary>
/// Use case that demonstrates write operation WITH UnitOfWork.
/// ConfigureExecutionInternal sets the UoW so the base class wraps
/// ExecuteInternalAsync in a transaction automatically.
/// </summary>
public sealed class RegisterSimpleAggregateRootUseCase
    : UseCaseBase<RegisterSimpleAggregateRootInput, RegisterSimpleAggregateRootOutput>,
    IRegisterSimpleAggregateRootUseCase
{
    private const string RegistrationFailedMessageCode = "RegisterSimpleAggregateRootUseCase.RegistrationFailed";

    private readonly IUnitOfWork _unitOfWork;
    private readonly ISimpleAggregateRootRepository _repository;

    public RegisterSimpleAggregateRootUseCase(
        ILogger<RegisterSimpleAggregateRootUseCase> logger,
        IUnitOfWork unitOfWork,
        ISimpleAggregateRootRepository repository
    ) : base(logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    protected override void ConfigureExecutionInternal(UseCaseExecutionOptions options)
    {
        // Write operation — wrap in UnitOfWork for transactional behavior.
        // The base class will call ExecuteInternalAsync inside UoW.ExecuteAsync,
        // committing on non-null result and rolling back on null.
        options.UnitOfWork = _unitOfWork;
    }

    protected override async Task<RegisterSimpleAggregateRootOutput?> ExecuteInternalAsync(
        ExecutionContext executionContext,
        RegisterSimpleAggregateRootInput input,
        CancellationToken cancellationToken
    )
    {
        var aggregateRoot = SimpleAggregateRoot.RegisterNew(
            executionContext,
            new RegisterNewInput(input.FirstName, input.LastName, input.BirthDate));

        if (aggregateRoot is null)
        {
            if (!executionContext.HasErrorMessages)
                executionContext.AddErrorMessage(code: RegistrationFailedMessageCode);

            return null;
        }

        var registered = await _repository.RegisterNewAsync(
            executionContext,
            aggregateRoot,
            cancellationToken);

        if (!registered)
        {
            if (!executionContext.HasErrorMessages)
                executionContext.AddErrorMessage(code: RegistrationFailedMessageCode);

            return null;
        }

        return new RegisterSimpleAggregateRootOutput(
            aggregateRoot.EntityInfo.Id.Value,
            aggregateRoot.FullName);
    }
}
