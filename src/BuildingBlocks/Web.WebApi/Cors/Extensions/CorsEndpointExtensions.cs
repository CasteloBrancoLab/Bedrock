using Microsoft.AspNetCore.Builder;

namespace Bedrock.BuildingBlocks.Web.WebApi.Cors.Extensions;

public static class CorsEndpointExtensions
{
    // Registra o middleware CORS no pipeline HTTP.
    //
    // Deve ser chamado DEPOIS de UseBedrockSecurityHeaders e UseBedrockRateLimiting,
    // e ANTES de UseAuthorization e MapControllers, para que o preflight (OPTIONS)
    // seja respondido antes de atingir autenticacao e routing.
    public static WebApplication UseBedrockCors(this WebApplication app)
    {
        app.UseCors();
        return app;
    }
}
