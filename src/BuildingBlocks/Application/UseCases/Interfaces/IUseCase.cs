namespace Bedrock.BuildingBlocks.Application.UseCases.Interfaces;

/// <summary>
/// Defines the contract for a use case that transforms an input into an output
/// within an execution context.
/// </summary>
/// <typeparam name="TInput">The type of the use case input. Must be a reference type.</typeparam>
/// <typeparam name="TOutput">The type of the use case output. Must be a reference type.</typeparam>
public interface IUseCase<in TInput, TOutput>
    where TInput : class
    where TOutput : class
{
    /// <summary>
    /// Executes the use case with the specified input.
    /// </summary>
    /// <param name="executionContext">The execution context containing correlation, tenancy, and tracing information.</param>
    /// <param name="input">The use case input data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The use case output if successful; otherwise, null.</returns>
    Task<TOutput?> ExecuteAsync(
        ExecutionContext executionContext,
        TInput input,
        CancellationToken cancellationToken);
}
