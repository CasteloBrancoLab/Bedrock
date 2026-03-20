using Bedrock.BuildingBlocks.Web.WebApi.ExecutionContextHeaders.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Bedrock.BuildingBlocks.Web.WebApi.ExecutionContextHeaders.Extensions;

public static class ExecutionContextHeadersEndpointExtensions
{
    // Registra o middleware que propaga os campos do ExecutionContext
    // como response headers em toda response HTTP.
    //
    // Deve ser chamado DEPOIS de UseBedrockSecurityHeaders e ANTES
    // de UseBedrockExceptionHandling para que toda response
    // (inclusive ProblemDetails) tenha os headers de rastreabilidade.
    public static WebApplication UseBedrockExecutionContextHeaders(this WebApplication app)
    {
        app.UseMiddleware<ExecutionContextHeadersMiddleware>();
        return app;
    }
}
