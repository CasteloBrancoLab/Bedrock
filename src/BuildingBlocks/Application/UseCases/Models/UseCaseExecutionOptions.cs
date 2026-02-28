using Bedrock.BuildingBlocks.Persistence.Abstractions.UnitOfWork.Interfaces;

namespace Bedrock.BuildingBlocks.Application.UseCases.Models;

/// <summary>
/// Configuration options for use case execution behavior.
/// </summary>
/// <remarks>
/// Set by concrete use cases via <see cref="UseCaseBase{TInput, TOutput}.ConfigureExecutionInternal"/>
/// to control cross-cutting concerns like unit of work orchestration.
/// </remarks>
public sealed class UseCaseExecutionOptions
{
    /// <summary>
    /// Gets or sets the unit of work to wrap the use case execution in a transaction.
    /// When null, the use case executes without transactional behavior.
    /// </summary>
    public IUnitOfWork? UnitOfWork { get; set; }
}
