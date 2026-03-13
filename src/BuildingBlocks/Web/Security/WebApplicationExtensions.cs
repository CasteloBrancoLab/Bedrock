using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Bedrock.BuildingBlocks.Web.Security;

public static class WebApplicationExtensions
{
    // Registra o middleware de security headers no pipeline HTTP.
    // Deve ser chamado no inicio do pipeline (antes de routing, auth, etc.)
    // para garantir que os headers sejam adicionados em todas as respostas,
    // incluindo erros e respostas curto-circuitadas.
    // O callback opcional permite sobrescrever ou adicionar headers apos os defaults.
    public static WebApplication UseBedrockSecurityHeaders(
        this WebApplication app,
        Action<IHeaderDictionary>? configure = null)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>(new object?[] { configure });
        return app;
    }
}
