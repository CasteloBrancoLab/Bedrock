using System.Diagnostics;
using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Bedrock.BuildingBlocks.Web.Logging.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bedrock.BuildingBlocks.Web.Logging.Middlewares;

// Middleware de logging estruturado para requests HTTP.
//
// Para cada request, loga:
// - Inicio: HTTP method, path, query, headers mascarados
// - Fim: status code, elapsed time em ms
//
// Usa logging scopes com correlationId e tenantId para que todos os logs
// dentro do request tenham o contexto de rastreabilidade automaticamente.
//
// Paths excluidos (health, swagger, scalar) nao geram logs para evitar ruido.
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly BedrockRequestLoggingOptions _options;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        BedrockRequestLoggingOptions options,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsExcludedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        using (_logger.BeginScope(BuildScopeData(context)))
        {
            LogRequestStart(context);

            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();

            LogRequestEnd(context, stopwatch.ElapsedMilliseconds);
        }
    }

    private bool IsExcludedPath(PathString path)
    {
        if (!path.HasValue)
        {
            return false;
        }

        foreach (var excluded in _options.ExcludedPaths)
        {
            if (path.Value.StartsWith(excluded, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void LogRequestStart(HttpContext context)
    {
        var request = context.Request;
        var maskedHeaders = SensitiveDataMasker.MaskHeaders(request.Headers, _options.SensitiveHeaders);

        _logger.LogInformation(
            "HTTP {Method} {Path}{QueryString} started. Headers: {@Headers}",
            request.Method,
            request.Path,
            request.QueryString,
            maskedHeaders);
    }

    private void LogRequestEnd(HttpContext context, long elapsedMs)
    {
        var statusCode = context.Response.StatusCode;
        var level = statusCode >= 500 ? LogLevel.Error
            : statusCode >= 400 ? LogLevel.Warning
            : LogLevel.Information;

        _logger.Log(
            level,
            "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
            context.Request.Method,
            context.Request.Path,
            statusCode,
            elapsedMs);
    }

    // Constroi o scope com todos os campos de rastreabilidade disponiveis nos headers,
    // alinhado com os mesmos campos do ExecutionContextScope usado pelo LogForDistributedTracing.
    // TenantName nao esta disponivel via header (apenas TenantCode/Id).
    private static Dictionary<string, object?> BuildScopeData(HttpContext context)
    {
        var headers = context.Request.Headers;

        return new Dictionary<string, object?>
        {
            ["CorrelationId"] = ExtractHeaderOrDefault(
                headers, ExecutionContextFactory.CorrelationIdHeaderName, context.TraceIdentifier),
            ["TenantCode"] = headers[ExecutionContextFactory.TenantIdHeaderName].FirstOrDefault(),
            ["ExecutionUser"] = ExtractHeaderOrDefault(
                headers, ExecutionContextFactory.ExecutionUserHeaderName, ExecutionContextFactory.DefaultExecutionUser),
            ["ExecutionOrigin"] = ExtractHeaderOrDefault(
                headers, ExecutionContextFactory.ExecutionOriginHeaderName, ExecutionContextFactory.DefaultExecutionOrigin),
            ["BusinessOperationCode"] = headers[ExecutionContextFactory.BusinessOperationCodeHeaderName].FirstOrDefault(),
        };
    }

    private static string ExtractHeaderOrDefault(IHeaderDictionary headers, string headerName, string defaultValue)
    {
        var value = headers[headerName].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
    }
}
