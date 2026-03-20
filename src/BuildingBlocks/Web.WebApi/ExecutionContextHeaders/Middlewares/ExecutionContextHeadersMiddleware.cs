using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Microsoft.AspNetCore.Http;

namespace Bedrock.BuildingBlocks.Web.WebApi.ExecutionContextHeaders.Middlewares;

// Propaga os campos do ExecutionContext como response headers em toda response HTTP,
// inclusive respostas de infraestrutura (ProblemDetails, rate limiting, etc.).
//
// Roda cedo no pipeline (apos SecurityHeaders, antes de ExceptionHandling)
// para que nenhuma response escape sem os headers de rastreabilidade.
//
// Headers propagados:
// - X-Correlation-Id: extraido do request ou auto-gerado
// - X-Tenant-Id: tenant do request (se presente)
// - X-Execution-User: usuario do request (default "anonymous")
// - X-Execution-Origin: origem do request (default "Api")
// - X-Business-Operation-Code: codigo da operacao de negocio (se presente)
public sealed class ExecutionContextHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public ExecutionContextHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestHeaders = context.Request.Headers;

        var correlationId = ExtractHeaderOrDefault(
            requestHeaders, ExecutionContextFactory.CorrelationIdHeaderName, Guid.NewGuid().ToString());
        var tenantId = requestHeaders[ExecutionContextFactory.TenantIdHeaderName].FirstOrDefault();
        var executionUser = ExtractHeaderOrDefault(
            requestHeaders, ExecutionContextFactory.ExecutionUserHeaderName, ExecutionContextFactory.DefaultExecutionUser);
        var executionOrigin = ExtractHeaderOrDefault(
            requestHeaders, ExecutionContextFactory.ExecutionOriginHeaderName, ExecutionContextFactory.DefaultExecutionOrigin);
        var businessOperationCode = requestHeaders[ExecutionContextFactory.BusinessOperationCodeHeaderName].FirstOrDefault();

        context.Response.OnStarting(() =>
        {
            var responseHeaders = context.Response.Headers;

            responseHeaders[ExecutionContextFactory.CorrelationIdHeaderName] = correlationId;
            responseHeaders[ExecutionContextFactory.ExecutionUserHeaderName] = executionUser;
            responseHeaders[ExecutionContextFactory.ExecutionOriginHeaderName] = executionOrigin;

            SetHeaderIfPresent(responseHeaders, ExecutionContextFactory.TenantIdHeaderName, tenantId);
            SetHeaderIfPresent(responseHeaders, ExecutionContextFactory.BusinessOperationCodeHeaderName, businessOperationCode);

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static string ExtractHeaderOrDefault(IHeaderDictionary headers, string headerName, string defaultValue)
    {
        var value = headers[headerName].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
    }

    private static void SetHeaderIfPresent(IHeaderDictionary headers, string headerName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            headers[headerName] = value;
        }
    }
}
