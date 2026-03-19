using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Bedrock.BuildingBlocks.Web.WebApi.ExceptionHandling.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Bedrock.BuildingBlocks.Web.WebApi.ExceptionHandling.Middlewares;

// Middleware global de exception handling que converte exceptions nao tratadas
// em respostas RFC 9457 Problem Details.
//
// Comportamento:
// - Resolve o status code via mapeamento (ExceptionStatusCodeMap) ou 500
// - Inclui correlationId e traceId como extensions no ProblemDetails
// - NUNCA expoe stack traces em producao (apenas com WithExceptionDetails + Development)
// - Loga a exception com nivel Error (ou Warning para client-cancelled)
// - Invoca callback Configure para enriquecimento customizado
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly BedrockExceptionHandlingOptions _options;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly bool _isDevelopment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        BedrockExceptionHandlingOptions options,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _options = options;
        _logger = logger;
        _isDevelopment = environment.IsDevelopment();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = ResolveStatusCode(exception);
        var problemDetails = BuildProblemDetails(context, exception, statusCode);

        LogException(exception, statusCode, problemDetails);

        _options.ConfigureProblemDetails?.Invoke(problemDetails, context, exception);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private int ResolveStatusCode(Exception exception)
    {
        var exceptionType = exception.GetType();

        // Verifica o tipo exato primeiro, depois tipos base na hierarquia.
        // Isso permite que BusinessRuleException : InvalidOperationException
        // tenha seu proprio mapeamento sem cair no default do pai.
        foreach (var mapping in _options.ExceptionStatusCodeMap)
        {
            if (mapping.Key == exceptionType)
            {
                return mapping.Value;
            }
        }

        foreach (var mapping in _options.ExceptionStatusCodeMap)
        {
            if (mapping.Key.IsAssignableFrom(exceptionType))
            {
                return mapping.Value;
            }
        }

        return StatusCodes.Status500InternalServerError;
    }

    private ProblemDetails BuildProblemDetails(
        HttpContext context,
        Exception exception,
        int statusCode)
    {
        var correlationId = ExtractCorrelationId(context);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = ShouldIncludeDetails() ? exception.Message : null,
            Instance = context.Request.Path,
        };

        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        if (ShouldIncludeDetails())
        {
            problemDetails.Extensions["exception"] = exception.ToString();
        }

        return problemDetails;
    }

    private bool ShouldIncludeDetails()
    {
        return _options.IncludeExceptionDetails && _isDevelopment;
    }

    private static string ExtractCorrelationId(HttpContext context)
    {
        var headerValue = context.Request.Headers[ExecutionContextFactory.CorrelationIdHeaderName]
            .FirstOrDefault();

        return !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue
            : context.TraceIdentifier;
    }

    private void LogException(Exception exception, int statusCode, ProblemDetails problemDetails)
    {
        var correlationId = problemDetails.Extensions["correlationId"];

        if (exception is OperationCanceledException)
        {
            _logger.LogWarning(
                "Request cancelled. CorrelationId: {CorrelationId}, Path: {Path}",
                correlationId,
                problemDetails.Instance);
            return;
        }

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. CorrelationId: {CorrelationId}, Path: {Path}, StatusCode: {StatusCode}",
                correlationId,
                problemDetails.Instance,
                statusCode);
            return;
        }

        _logger.LogWarning(
            "Handled exception. CorrelationId: {CorrelationId}, Path: {Path}, StatusCode: {StatusCode}, Message: {Message}",
            correlationId,
            problemDetails.Instance,
            statusCode,
            exception.Message);
    }
}
