namespace Bedrock.BuildingBlocks.Resilience.Models;

/// <summary>
/// Encapsulates the outcome of a resilience policy execution, including the result value,
/// success status, failure reason, and any captured exception.
/// </summary>
/// <typeparam name="TOutput">The type of the handler's return value.</typeparam>
public sealed class ResiliencePolicyExecutionResult<TOutput>
{
    /// <summary>
    /// Gets a value indicating whether the execution completed successfully
    /// (either primary success or fallback success).
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result was produced by a fallback handler
    /// rather than the primary handler.
    /// </summary>
    public bool IsFallback { get; }

    /// <summary>
    /// Gets the result value when the execution succeeded; otherwise, <c>default</c>.
    /// </summary>
    public TOutput? Value { get; }

    /// <summary>
    /// Gets the exception that caused the failure, if any.
    /// When <see cref="IsFallback"/> is <c>true</c>, this contains the original exception that triggered the fallback.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the reason the execution failed. <see cref="ResiliencePolicyFailureReason.None"/> when successful without fallback.
    /// When <see cref="IsFallback"/> is <c>true</c>, this contains the original failure reason that triggered the fallback.
    /// </summary>
    public ResiliencePolicyFailureReason FailureReason { get; }

    private ResiliencePolicyExecutionResult(
        bool isSuccess,
        bool isFallback,
        TOutput? value,
        Exception? exception,
        ResiliencePolicyFailureReason failureReason)
    {
        IsSuccess = isSuccess;
        IsFallback = isFallback;
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
            isFallback: false,
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
            isFallback: false,
            value: default,
            exception: exception,
            failureReason: failureReason);
    }

    /// <summary>
    /// Creates a fallback result — the primary handler failed, but the fallback produced a value.
    /// <see cref="IsSuccess"/> is <c>true</c>, <see cref="IsFallback"/> is <c>true</c>,
    /// and <see cref="FailureReason"/> contains the original failure reason.
    /// </summary>
    public static ResiliencePolicyExecutionResult<TOutput> CreateFallback(
        TOutput value,
        ResiliencePolicyFailureReason originalReason,
        Exception? exception = null)
    {
        return new ResiliencePolicyExecutionResult<TOutput>(
            isSuccess: true,
            isFallback: true,
            value: value,
            exception: exception,
            failureReason: originalReason);
    }
}
