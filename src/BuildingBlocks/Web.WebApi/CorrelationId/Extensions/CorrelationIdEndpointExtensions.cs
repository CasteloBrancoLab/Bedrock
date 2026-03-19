using Bedrock.BuildingBlocks.Web.WebApi.CorrelationId.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Bedrock.BuildingBlocks.Web.WebApi.CorrelationId.Extensions;

public static class CorrelationIdEndpointExtensions
{
    // Registra o middleware de CorrelationId no pipeline HTTP.
    //
    // Deve ser chamado DEPOIS de UseBedrockSecurityHeaders e ANTES
    // de UseBedrockExceptionHandling para que toda response
    // (inclusive ProblemDetails) tenha o header X-Correlation-Id.
    public static WebApplication UseBedrockCorrelationId(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }
}
