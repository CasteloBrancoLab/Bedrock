using Microsoft.AspNetCore.Builder;

namespace Bedrock.BuildingBlocks.Web.WebApi.RateLimiting;

public static class RateLimitingEndpointExtensions
{
    // Registra o middleware de rate limiting no pipeline HTTP.
    //
    // Deve ser chamado ANTES de UseAuthorization e MapControllers
    // para que requests sejam rejeitadas antes de consumir recursos
    // de autenticacao e processamento de controllers.
    //
    // Deve ser chamado DEPOIS de UseBedrockSecurityHeaders para que
    // mesmo respostas 429 tenham os headers de seguranca.
    public static WebApplication UseBedrockRateLimiting(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }
}
