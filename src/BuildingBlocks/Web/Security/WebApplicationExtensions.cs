using Microsoft.AspNetCore.Builder;

namespace Bedrock.BuildingBlocks.Web.Security;

public static class WebApplicationExtensions
{
    // Registra o middleware de security headers no pipeline HTTP.
    // Deve ser chamado no inicio do pipeline (antes de routing, auth, etc.)
    // para garantir que os headers sejam adicionados em todas as respostas,
    // incluindo erros e respostas curto-circuitadas.
    public static WebApplication UseBedrockSecurityHeaders(this WebApplication app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        return app;
    }
}
