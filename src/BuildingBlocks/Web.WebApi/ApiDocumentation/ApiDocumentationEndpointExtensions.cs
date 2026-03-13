using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation;

public static class ApiDocumentationEndpointExtensions
{
    private const string ScalarBasePath = "/scalar/v1";

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

        app.UseSwagger();

        app.MapScalarApiReference(scalar =>
        {
            ConfigureScalarDefaults(scalar);
            options?.ConfigureScalar?.Invoke(scalar);
        });

        MapRootRedirect(app);

        return app;
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
