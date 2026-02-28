using Bedrock.BuildingBlocks.Application.UseCases;
using Bedrock.BuildingBlocks.Application.UseCases.Models;
using Bedrock.BuildingBlocks.Core.Ids;
using Microsoft.Extensions.Logging;
using Templates.Application.UseCases.GetSimpleAggregateRootById.Interfaces;
using Templates.Application.UseCases.GetSimpleAggregateRootById.Models;
using Templates.Domain.Repositories.Interfaces;

namespace Templates.Application.UseCases.GetSimpleAggregateRootById;

/// <summary>
/// Use case that demonstrates read operation WITHOUT UnitOfWork.
/// ConfigureExecutionInternal is a no-op — no transactional behavior needed for reads.
/// </summary>
public sealed class GetSimpleAggregateRootByIdUseCase
    : UseCaseBase<GetSimpleAggregateRootByIdInput, GetSimpleAggregateRootByIdOutput>,
    IGetSimpleAggregateRootByIdUseCase
{
    private const string NotFoundMessageCode = "GetSimpleAggregateRootByIdUseCase.NotFound";

    private readonly ISimpleAggregateRootRepository _repository;

    public GetSimpleAggregateRootByIdUseCase(
        ILogger<GetSimpleAggregateRootByIdUseCase> logger,
        ISimpleAggregateRootRepository repository
    ) : base(logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    protected override void ConfigureExecutionInternal(UseCaseExecutionOptions options)
    {
        // Read-only operation — no UnitOfWork needed.
    }

    protected override async Task<GetSimpleAggregateRootByIdOutput?> ExecuteInternalAsync(
        ExecutionContext executionContext,
        GetSimpleAggregateRootByIdInput input,
        CancellationToken cancellationToken
    )
    {
        var aggregateRoot = await _repository.GetByIdAsync(
            executionContext,
            Id.CreateFromExistingInfo(input.Id),
            cancellationToken);

        if (aggregateRoot is null)
        {
            if (!executionContext.HasErrorMessages)
                executionContext.AddErrorMessage(code: NotFoundMessageCode);

            return null;
        }

        return new GetSimpleAggregateRootByIdOutput(
            aggregateRoot.EntityInfo.Id.Value,
            aggregateRoot.FirstName,
            aggregateRoot.LastName,
            aggregateRoot.FullName,
            aggregateRoot.BirthDate.Value);
    }
}
