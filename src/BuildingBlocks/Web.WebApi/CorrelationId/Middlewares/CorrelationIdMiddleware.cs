using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Microsoft.AspNetCore.Http;

namespace Bedrock.BuildingBlocks.Web.WebApi.CorrelationId.Middlewares;

// Garante que toda response HTTP contenha o header X-Correlation-Id,
// inclusive respostas de infraestrutura (ProblemDetails, rate limiting, etc.).
//
// Roda cedo no pipeline (apos SecurityHeaders, antes de ExceptionHandling)
// para que nenhuma response escape sem o header.
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ExtractOrGenerateCorrelationId(context);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[ExecutionContextFactory.CorrelationIdHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static string ExtractOrGenerateCorrelationId(HttpContext context)
    {
        var headerValue = context.Request.Headers[ExecutionContextFactory.CorrelationIdHeaderName]
            .FirstOrDefault();

        return !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue
            : Id.GenerateNewId().Value.ToString();
    }
}
