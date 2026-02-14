using System.Collections;
using System.Runtime.CompilerServices;
using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Microsoft.Extensions.Logging;

namespace Bedrock.BuildingBlocks.Observability.ExtensionMethods;

/// <summary>
/// Extension methods for <see cref="ILogger"/> that provide distributed tracing capabilities
/// by automatically including <see cref="ExecutionContext"/> information in log scopes.
/// </summary>
/// <remarks>
/// <para>
/// This class provides zero-allocation logging methods by using generic overloads for 0-3 arguments,
/// avoiding the <c>params object[]</c> array allocation in hot paths. A fallback with <c>params</c>
/// is provided for cases with 4+ arguments.
/// </para>
/// <para>
/// The scope data is provided via <see cref="ExecutionContextScope"/>, a struct that implements
/// <see cref="IReadOnlyList{T}"/> without heap allocation.
/// </para>
/// </remarks>
public static class LoggerExtensionMethods
{
#pragma warning disable CS8601 // False positive - C# 14 extension member syntax not fully supported by nullable analysis
    extension(ILogger logger)
    {
        // ================================
        // LogTraceForDistributedTracing
        // ================================

        /// <summary>
        /// Logs a trace message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        public void LogTraceForDistributedTracing(
            ExecutionContext executionContext,
            string message)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Trace, exception: null, message, []);
        }

        /// <summary>
        /// Logs a trace message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        public void LogTraceForDistributedTracing<T0>(
            ExecutionContext executionContext,
            string message,
            T0 arg0)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Trace, exception: null, message, [arg0]);
        }

        /// <summary>
        /// Logs a trace message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        public void LogTraceForDistributedTracing<T0, T1>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Trace, exception: null, message, [arg0, arg1]);
        }

        /// <summary>
        /// Logs a trace message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <typeparam name="T2">The type of the third argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        /// <param name="arg2">The third argument to format the message template.</param>
        public void LogTraceForDistributedTracing<T0, T1, T2>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1,
            T2 arg2)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Trace, exception: null, message, [arg0, arg1, arg2]);
        }

        /// <summary>
        /// Logs a trace message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">The arguments to format the message template.</param>
        public void LogTraceForDistributedTracing(
            ExecutionContext executionContext,
            string message,
            params object[] args)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Trace, exception: null, message, args);
        }

        // ================================
        // LogDebugForDistributedTracing
        // ================================

        /// <summary>
        /// Logs a debug message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        public void LogDebugForDistributedTracing(
            ExecutionContext executionContext,
            string message)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Debug, exception: null, message, []);
        }

        /// <summary>
        /// Logs a debug message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        public void LogDebugForDistributedTracing<T0>(
            ExecutionContext executionContext,
            string message,
            T0 arg0)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Debug, exception: null, message, [arg0]);
        }

        /// <summary>
        /// Logs a debug message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        public void LogDebugForDistributedTracing<T0, T1>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Debug, exception: null, message, [arg0, arg1]);
        }

        /// <summary>
        /// Logs a debug message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <typeparam name="T2">The type of the third argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        /// <param name="arg2">The third argument to format the message template.</param>
        public void LogDebugForDistributedTracing<T0, T1, T2>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1,
            T2 arg2)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Debug, exception: null, message, [arg0, arg1, arg2]);
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
            params object[] args)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Debug, exception: null, message, args);
        }

        // ================================
        // LogInformationForDistributedTracing
        // ================================

        /// <summary>
        /// Logs an information message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        public void LogInformationForDistributedTracing(
            ExecutionContext executionContext,
            string message)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Information, exception: null, message, []);
        }

        /// <summary>
        /// Logs an information message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        public void LogInformationForDistributedTracing<T0>(
            ExecutionContext executionContext,
            string message,
            T0 arg0)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Information, exception: null, message, [arg0]);
        }

        /// <summary>
        /// Logs an information message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        public void LogInformationForDistributedTracing<T0, T1>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Information, exception: null, message, [arg0, arg1]);
        }

        /// <summary>
        /// Logs an information message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <typeparam name="T2">The type of the third argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        /// <param name="arg2">The third argument to format the message template.</param>
        public void LogInformationForDistributedTracing<T0, T1, T2>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1,
            T2 arg2)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Information, exception: null, message, [arg0, arg1, arg2]);
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
            params object[] args)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Information, exception: null, message, args);
        }

        // ================================
        // LogWarningForDistributedTracing
        // ================================

        /// <summary>
        /// Logs a warning message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        public void LogWarningForDistributedTracing(
            ExecutionContext executionContext,
            string message)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Warning, exception: null, message, []);
        }

        /// <summary>
        /// Logs a warning message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        public void LogWarningForDistributedTracing<T0>(
            ExecutionContext executionContext,
            string message,
            T0 arg0)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Warning, exception: null, message, [arg0]);
        }

        /// <summary>
        /// Logs a warning message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        public void LogWarningForDistributedTracing<T0, T1>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Warning, exception: null, message, [arg0, arg1]);
        }

        /// <summary>
        /// Logs a warning message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <typeparam name="T2">The type of the third argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        /// <param name="arg2">The third argument to format the message template.</param>
        public void LogWarningForDistributedTracing<T0, T1, T2>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1,
            T2 arg2)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Warning, exception: null, message, [arg0, arg1, arg2]);
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
            params object[] args)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Warning, exception: null, message, args);
        }

        // ================================
        // LogErrorForDistributedTracing
        // ================================

        /// <summary>
        /// Logs an error message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        public void LogErrorForDistributedTracing(
            ExecutionContext executionContext,
            string message)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception: null, message, []);
        }

        /// <summary>
        /// Logs an error message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        public void LogErrorForDistributedTracing<T0>(
            ExecutionContext executionContext,
            string message,
            T0 arg0)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception: null, message, [arg0]);
        }

        /// <summary>
        /// Logs an error message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        public void LogErrorForDistributedTracing<T0, T1>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception: null, message, [arg0, arg1]);
        }

        /// <summary>
        /// Logs an error message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <typeparam name="T2">The type of the third argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        /// <param name="arg2">The third argument to format the message template.</param>
        public void LogErrorForDistributedTracing<T0, T1, T2>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1,
            T2 arg2)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception: null, message, [arg0, arg1, arg2]);
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
            params object[] args)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception: null, message, args);
        }

        // ================================
        // LogCriticalForDistributedTracing
        // ================================

        /// <summary>
        /// Logs a critical message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        public void LogCriticalForDistributedTracing(
            ExecutionContext executionContext,
            string message)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Critical, exception: null, message, []);
        }

        /// <summary>
        /// Logs a critical message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        public void LogCriticalForDistributedTracing<T0>(
            ExecutionContext executionContext,
            string message,
            T0 arg0)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Critical, exception: null, message, [arg0]);
        }

        /// <summary>
        /// Logs a critical message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        public void LogCriticalForDistributedTracing<T0, T1>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Critical, exception: null, message, [arg0, arg1]);
        }

        /// <summary>
        /// Logs a critical message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <typeparam name="T2">The type of the third argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        /// <param name="arg2">The third argument to format the message template.</param>
        public void LogCriticalForDistributedTracing<T0, T1, T2>(
            ExecutionContext executionContext,
            string message,
            T0 arg0,
            T1 arg1,
            T2 arg2)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Critical, exception: null, message, [arg0, arg1, arg2]);
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
            params object[] args)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Critical, exception: null, message, args);
        }

        // ================================
        // LogExceptionForDistributedTracing
        // ================================

        /// <summary>
        /// Logs an exception with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The message template to log.</param>
        public void LogExceptionForDistributedTracing(
            ExecutionContext executionContext,
            Exception exception,
            string message)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception, message, []);
        }

        /// <summary>
        /// Logs an exception with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        public void LogExceptionForDistributedTracing<T0>(
            ExecutionContext executionContext,
            Exception exception,
            string message,
            T0 arg0)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception, message, [arg0]);
        }

        /// <summary>
        /// Logs an exception with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        public void LogExceptionForDistributedTracing<T0, T1>(
            ExecutionContext executionContext,
            Exception exception,
            string message,
            T0 arg0,
            T1 arg1)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception, message, [arg0, arg1]);
        }

        /// <summary>
        /// Logs an exception with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <typeparam name="T2">The type of the third argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        /// <param name="arg2">The third argument to format the message template.</param>
        public void LogExceptionForDistributedTracing<T0, T1, T2>(
            ExecutionContext executionContext,
            Exception exception,
            string message,
            T0 arg0,
            T1 arg1,
            T2 arg2)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception, message, [arg0, arg1, arg2]);
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
            params object[] args)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception, message, args);
        }

        /// <summary>
        /// Logs an exception with distributed tracing context from the execution context,
        /// using the exception message as the log message.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="exception">The exception to log.</param>
        public void LogExceptionForDistributedTracing(
            ExecutionContext executionContext,
            Exception exception)
        {
            LogForDistributedTracingCore(logger, executionContext, LogLevel.Error, exception, message: exception.Message, []);
        }

        // ================================
        // LogForDistributedTracing (generic core method)
        // ================================

        /// <summary>
        /// Logs a message with distributed tracing context from the execution context.
        /// </summary>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception to log, if any.</param>
        /// <param name="message">The message template to log.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="executionContext"/> is null.</exception>
        public void LogForDistributedTracing(
            ExecutionContext executionContext,
            LogLevel logLevel,
            Exception? exception,
            string message)
        {
            LogForDistributedTracingCore(logger, executionContext, logLevel, exception, message, []);
        }

        /// <summary>
        /// Logs a message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception to log, if any.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="executionContext"/> is null.</exception>
        public void LogForDistributedTracing<T0>(
            ExecutionContext executionContext,
            LogLevel logLevel,
            Exception? exception,
            string message,
            T0 arg0)
        {
            LogForDistributedTracingCore(logger, executionContext, logLevel, exception, message, [arg0]);
        }

        /// <summary>
        /// Logs a message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception to log, if any.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="executionContext"/> is null.</exception>
        public void LogForDistributedTracing<T0, T1>(
            ExecutionContext executionContext,
            LogLevel logLevel,
            Exception? exception,
            string message,
            T0 arg0,
            T1 arg1)
        {
            LogForDistributedTracingCore(logger, executionContext, logLevel, exception, message, [arg0, arg1]);
        }

        /// <summary>
        /// Logs a message with distributed tracing context from the execution context.
        /// </summary>
        /// <typeparam name="T0">The type of the first argument.</typeparam>
        /// <typeparam name="T1">The type of the second argument.</typeparam>
        /// <typeparam name="T2">The type of the third argument.</typeparam>
        /// <param name="executionContext">The execution context containing correlation and tracing information.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="exception">The exception to log, if any.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="arg0">The first argument to format the message template.</param>
        /// <param name="arg1">The second argument to format the message template.</param>
        /// <param name="arg2">The third argument to format the message template.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="executionContext"/> is null.</exception>
        public void LogForDistributedTracing<T0, T1, T2>(
            ExecutionContext executionContext,
            LogLevel logLevel,
            Exception? exception,
            string message,
            T0 arg0,
            T1 arg1,
            T2 arg2)
        {
            LogForDistributedTracingCore(logger, executionContext, logLevel, exception, message, [arg0, arg1, arg2]);
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
            params object[] args)
        {
            LogForDistributedTracingCore(logger, executionContext, logLevel, exception, message, args);
        }
    }
#pragma warning restore CS8601

    /// <summary>
    /// Core logging implementation that handles the actual logging with distributed tracing context.
    /// </summary>
    /// <remarks>
    /// This method is marked with <see cref="MethodImplAttribute"/> with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// to encourage the JIT to inline the early-exit check for disabled log levels.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogForDistributedTracingCore(
        ILogger logger,
        ExecutionContext executionContext,
        LogLevel logLevel,
        Exception? exception,
        string message,
        object[] args)
    {
        if (!logger.IsEnabled(logLevel))
            return;

        ArgumentNullException.ThrowIfNull(executionContext);

        using (logger.BeginScope(new ExecutionContextScope(executionContext)))
        {
#pragma warning disable CA1848, CA2254 // Message template is intentionally dynamic in this wrapper method
            // CS003 disable once : implementação core do pattern ForDistributedTracing — único ponto legítimo que chama ILogger.Log
            logger.Log(logLevel, exception, message, args);
#pragma warning restore CA1848, CA2254
        }
    }
}

/// <summary>
/// A zero-allocation struct that provides execution context data for logging scopes.
/// </summary>
/// <remarks>
/// <para>
/// This struct implements <see cref="IReadOnlyList{T}"/> to be compatible with
/// <see cref="ILogger.BeginScope{TState}(TState)"/> without allocating a dictionary on each call.
/// </para>
/// <para>
/// The Microsoft.Extensions.Logging infrastructure recognizes <see cref="IReadOnlyList{T}"/>
/// of <see cref="KeyValuePair{TKey, TValue}"/> as structured logging data.
/// </para>
/// </remarks>
public readonly struct ExecutionContextScope : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly ExecutionContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionContextScope"/> struct.
    /// </summary>
    /// <param name="context">The execution context to expose as scope data.</param>
    public ExecutionContextScope(ExecutionContext context) => _context = context;

    /// <summary>
    /// Gets the number of key-value pairs in the scope.
    /// </summary>
    public int Count => 7;

    /// <summary>
    /// Gets the key-value pair at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The key-value pair at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
    public KeyValuePair<string, object?> this[int index] => index switch
    {
        0 => new("Timestamp", _context.Timestamp),
        1 => new("CorrelationId", _context.CorrelationId),
        2 => new("TenantCode", _context.TenantInfo.Code),
        3 => new("TenantName", _context.TenantInfo.Name),
        4 => new("ExecutionUser", _context.ExecutionUser),
        5 => new("ExecutionOrigin", _context.ExecutionOrigin),
        6 => new("BusinessOperationCode", _context.BusinessOperationCode),
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be between 0 and {Count - 1}.")
    };

    /// <summary>
    /// Returns an enumerator that iterates through the key-value pairs.
    /// </summary>
    /// <returns>An enumerator for the key-value pairs.</returns>
    public Enumerator GetEnumerator() => new(this);

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator() => new Enumerator(this);

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    /// <summary>
    /// An enumerator for <see cref="ExecutionContextScope"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<KeyValuePair<string, object?>>
    {
        private readonly ExecutionContextScope _scope;
        private int _index;

        internal Enumerator(ExecutionContextScope scope)
        {
            _scope = scope;
            _index = -1;
        }

        /// <inheritdoc/>
        public readonly KeyValuePair<string, object?> Current => _scope[_index];

        /// <inheritdoc/>
        readonly object IEnumerator.Current => Current;

        /// <inheritdoc/>
        public bool MoveNext()
        {
            int nextIndex = _index + 1;
            if (nextIndex < _scope.Count)
            {
                _index = nextIndex;
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public void Reset() => _index = -1;

        /// <inheritdoc/>
        public readonly void Dispose() { }
    }
}
