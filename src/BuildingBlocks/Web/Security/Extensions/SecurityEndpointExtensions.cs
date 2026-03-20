using Bedrock.BuildingBlocks.Web.Security.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Bedrock.BuildingBlocks.Web.Security.Extensions;

public static class SecurityEndpointExtensions
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
        if (configure is not null)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>(configure);
        }
        else
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
        }

        return app;
    }
}
