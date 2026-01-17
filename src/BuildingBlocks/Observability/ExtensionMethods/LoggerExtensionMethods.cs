using Microsoft.Extensions.Logging;

namespace Bedrock.BuildingBlocks.Observability.ExtensionMethods;

/// <summary>
/// Extension methods for <see cref="ILogger"/> that provide distributed tracing capabilities
/// by automatically including <see cref="ExecutionContext"/> information in log scopes.
/// </summary>
public static class LoggerExtensionMethods
{
    extension(ILogger logger)
    {
        /// <summary>
        /// Logs a trace message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">The arguments to format the message template.</param>
        public void LogTraceForDistributedTracing(
            ExecutionContext executionContext,
            string message,
            params object[] args
        )
        {
            logger.LogForDistributedTracing(executionContext, LogLevel.Trace, exception: null, message, args);
        }

        /// <summary>
        /// Logs a debug message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">The arguments to format the message template.</param>
        public void LogDebugForDistributedTracing(
            ExecutionContext executionContext,
            string message,
            params object[] args
        )
        {
            logger.LogForDistributedTracing(executionContext, LogLevel.Debug, exception: null, message, args);
        }

        /// <summary>
        /// Logs an information message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">The arguments to format the message template.</param>
        public void LogInformationForDistributedTracing(
            ExecutionContext executionContext,
            string message,
            params object[] args
        )
        {
            logger.LogForDistributedTracing(executionContext, LogLevel.Information, exception: null, message, args);
        }

        /// <summary>
        /// Logs a warning message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">The arguments to format the message template.</param>
        public void LogWarningForDistributedTracing(
            ExecutionContext executionContext,
            string message,
            params object[] args
        )
        {
            logger.LogForDistributedTracing(executionContext, LogLevel.Warning, exception: null, message, args);
        }

        /// <summary>
        /// Logs an error message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">The arguments to format the message template.</param>
        public void LogErrorForDistributedTracing(
            ExecutionContext executionContext,
            string message,
            params object[] args
        )
        {
            logger.LogForDistributedTracing(executionContext, LogLevel.Error, exception: null, message, args);
        }

        /// <summary>
        /// Logs a critical message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">The arguments to format the message template.</param>
        public void LogCriticalForDistributedTracing(
            ExecutionContext executionContext,
            string message,
            params object[] args
        )
        {
            logger.LogForDistributedTracing(executionContext, LogLevel.Critical, exception: null, message, args);
        }

        /// <summary>
        /// Logs an exception with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">The arguments to format the message template.</param>
        public void LogExceptionForDistributedTracing(
            ExecutionContext executionContext,
            Exception exception,
            string message,
            params object[] args
        )
        {
            logger.LogForDistributedTracing(executionContext, LogLevel.Error, exception, message, args);
        }

        /// <summary>
        /// Logs an exception with distributed tracing context from the execution context,
        /// using the exception message as the log message.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="exception">The exception to log.</param>
        public void LogExceptionForDistributedTracing(
            ExecutionContext executionContext,
            Exception exception
        )
        {
            logger.LogForDistributedTracing(executionContext, LogLevel.Error, exception, message: exception.Message);
        }

        /// <summary>
        /// Logs a message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception to log, if any.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">The arguments to format the message template.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="executionContext"/> is null.</exception>
        public void LogForDistributedTracing(
            ExecutionContext executionContext,
            LogLevel logLevel,
            Exception? exception,
            string message,
            params object[] args
        )
        {
            if (!logger.IsEnabled(logLevel))
                return;

            ArgumentNullException.ThrowIfNull(executionContext);

            using (logger.BeginScope(executionContext.ToDictionary()))
            {
#pragma warning disable CA1848, CA2254 // Message template is intentionally dynamic in this wrapper method
                logger.Log(logLevel, exception, message, args);
#pragma warning restore CA1848, CA2254
            }
        }
    }
}
