using Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Extensions;

public static class ApiDocumentationEndpointExtensions
{
    private const string ScalarBasePath = "/scalar/v1";
    private const string ScalarPathPrefix = "/scalar";
    private const string SwaggerPathPrefix = "/swagger";

    // Registra o middleware do Swashbuckle (UseSwagger) para servir a spec JSON
    // e o Scalar para renderizar o UI interativo de documentacao.
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

    private static void ConfigureScalarDefaults(ScalarOptions scalar)
    {
        scalar.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json";
    }

    private static void MapRootRedirect(WebApplication app)
    {
        app.MapGet("/", () => Results.Redirect(ScalarBasePath, permanent: true))
            .ExcludeFromDescription();
    }
}
