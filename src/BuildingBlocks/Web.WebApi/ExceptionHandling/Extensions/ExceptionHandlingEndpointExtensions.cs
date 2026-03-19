using Bedrock.BuildingBlocks.Web.WebApi.ExceptionHandling.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Bedrock.BuildingBlocks.Web.WebApi.ExceptionHandling.Extensions;

public static class ExceptionHandlingEndpointExtensions
{
    // Registra o middleware de exception handling no pipeline HTTP.
    //
    // Deve ser chamado DEPOIS de UseBedrockSecurityHeaders (para que respostas
    // de erro tenham os headers de seguranca) e ANTES de todos os outros
    // middlewares (para capturar exceptions de qualquer camada).
    public static WebApplication UseBedrockExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
