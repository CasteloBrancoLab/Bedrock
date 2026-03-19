using Microsoft.AspNetCore.Builder;

namespace Bedrock.BuildingBlocks.Web.WebApi.OutputCaching.Extensions;

public static class OutputCachingEndpointExtensions
{
    // Registra o middleware de output caching no pipeline HTTP.
    //
    // Deve ser chamado DEPOIS de UseBedrockCors (para que respostas
    // cacheadas incluam headers CORS) e ANTES de UseAuthorization
    // (para evitar cache de respostas autenticadas sem vary-by).
    public static WebApplication UseBedrockOutputCaching(this WebApplication app)
    {
        app.UseOutputCache();
        return app;
    }
}
