using Bedrock.BuildingBlocks.Web.Logging.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Bedrock.BuildingBlocks.Web.Logging.Extensions;

public static class RequestLoggingEndpointExtensions
{
    // Registra o middleware de request logging no pipeline HTTP.
    //
    // Deve ser chamado DEPOIS de UseBedrockExceptionHandling (para que
    // o elapsed time inclua o tempo de processamento de erros) e ANTES
    // de UseBedrockRateLimiting (para logar requests que seriam rejeitados).
    public static WebApplication UseBedrockRequestLogging(this WebApplication app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}
