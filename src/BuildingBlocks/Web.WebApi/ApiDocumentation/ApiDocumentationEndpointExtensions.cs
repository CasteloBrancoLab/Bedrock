using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation;

public static class ApiDocumentationEndpointExtensions
{
    private const string ScalarBasePath = "/scalar/v1";
    private const string ScalarPathPrefix = "/scalar";
    private const string SwaggerPathPrefix = "/swagger";

    // Registra o middleware do Swashbuckle (UseSwagger) para servir a spec JSON
    // e o Scalar para renderizar o UI interativo de documentacao.
    //
    // O Scalar e configurado para apontar para o endpoint do Swashbuckle
    // (/swagger/{documentName}/swagger.json) em vez do padrao do .NET
    // (/openapi/{documentName}.json), pois o Swashbuckle gera a spec
    // respeitando IOutboundParameterTransformer e API versioning corretamente.
    //
    // Redireciona a raiz (/) para o Scalar, pois a raiz nao serve nenhum
    // conteudo e o Scalar e o ponto de entrada natural para desenvolvedores.
    //
    // Por default, so e ativado em Development. O parametro enableInAllEnvironments
    // permite sobrescrever esse comportamento para cenarios como staging com
    // documentacao acessivel.
    public static WebApplication UseBedrockApiDocumentation(
        this WebApplication app,
        BedrockApiDocumentationOptions? options = null,
        bool enableInAllEnvironments = false)
    {
        if (!enableInAllEnvironments && !app.Environment.IsDevelopment())
        {
            return app;
        }

        RelaxSecurityHeadersForDocumentation(app);

        app.UseSwagger();

        app.MapScalarApiReference(scalar =>
        {
            ConfigureScalarDefaults(scalar);
            options?.ConfigureScalar?.Invoke(scalar);
        });

        MapRootRedirect(app);

        return app;
    }

    // Sobrescreve o Content-Security-Policy para os paths de documentacao
    // (/scalar e /swagger). O Scalar serve HTML com scripts e styles inline
    // que sao bloqueados pelo CSP padrao `default-src 'self'`.
    //
    // O middleware de seguranca continua aplicando todos os outros headers
    // (X-Content-Type-Options, X-Frame-Options, etc.) normalmente.
    // Apenas o CSP e relaxado, e somente para os paths de documentacao.
    private static void RelaxSecurityHeadersForDocumentation(WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (IsDocumentationPath(context.Request.Path))
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["Content-Security-Policy"] =
                        "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                        "font-src 'self' https://cdn.jsdelivr.net; " +
                        "img-src 'self' data:; " +
                        "connect-src 'self'";
                    return Task.CompletedTask;
                });
            }

            await next();
        });
    }

    private static bool IsDocumentationPath(PathString path)
    {
        return path.StartsWithSegments(ScalarPathPrefix, StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments(SwaggerPathPrefix, StringComparison.OrdinalIgnoreCase);
    }

    // Aponta o Scalar para o endpoint do Swashbuckle em vez do default
    // do Microsoft.AspNetCore.OpenApi. O formato /swagger/{documentName}/swagger.json
    // e o padrao do Swashbuckle onde {0} e substituido pelo documentName
    // (group name da versao, ex: v1, v2).
    private static void ConfigureScalarDefaults(ScalarOptions scalar)
    {
        scalar.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json";
    }

    // Redireciona GET / para o Scalar UI. Usa redirect permanente (301)
    // para que browsers e proxies cacheiem o redirect e nao batam
    // no servidor a cada acesso.
    private static void MapRootRedirect(WebApplication app)
    {
        app.MapGet("/", () => Results.Redirect(ScalarBasePath, permanent: true))
            .ExcludeFromDescription();
    }
}
