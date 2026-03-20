namespace Bedrock.BuildingBlocks.Resilience.Models;

/// <summary>
/// Encapsulates the outcome of a resilience policy execution, including the result value,
/// success status, failure reason, and any captured exception.
/// </summary>
/// <typeparam name="TOutput">The type of the handler's return value.</typeparam>
public sealed class ResiliencePolicyExecutionResult<TOutput>
{
    /// <summary>
    /// Gets a value indicating whether the execution completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the result value when the execution succeeded; otherwise, <c>default</c>.
    /// </summary>
    public TOutput? Value { get; }

    /// <summary>
    /// Gets the exception that caused the failure, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the reason the execution failed. <see cref="ResiliencePolicyFailureReason.None"/> when successful.
    /// </summary>
    public ResiliencePolicyFailureReason FailureReason { get; }

    private ResiliencePolicyExecutionResult(
        bool isSuccess,
        TOutput? value,
        Exception? exception,
        ResiliencePolicyFailureReason failureReason)
    {
        IsSuccess = isSuccess;
        Value = value;
        Exception = exception;
        FailureReason = failureReason;
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    public static ResiliencePolicyExecutionResult<TOutput> CreateSuccess(TOutput value)
    {
        return new ResiliencePolicyExecutionResult<TOutput>(
            isSuccess: true,
            value: value,
            exception: null,
            failureReason: ResiliencePolicyFailureReason.None);
    }

    /// <summary>
    /// Creates a failed result with the given reason and optional exception.
    /// </summary>
    public static ResiliencePolicyExecutionResult<TOutput> CreateFailure(
        ResiliencePolicyFailureReason failureReason,
        Exception? exception = null)
    {
        return new ResiliencePolicyExecutionResult<TOutput>(
            isSuccess: false,
            value: default,
            exception: exception,
            failureReason: failureReason);
    }
}
