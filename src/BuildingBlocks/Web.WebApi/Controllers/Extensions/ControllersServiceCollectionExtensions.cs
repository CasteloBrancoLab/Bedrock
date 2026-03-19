using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Bedrock.BuildingBlocks.Web.WebApi.Controllers.Models;
using Bedrock.BuildingBlocks.Web.WebApi.Conventions;
using Bedrock.BuildingBlocks.Web.WebApi.Envelope.Translation;
using Bedrock.BuildingBlocks.Web.WebApi.Serialization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.WebApi.Controllers.Extensions;

public static class ControllersServiceCollectionExtensions
{
    // Registra controllers com as conventions padrao do Bedrock:
    // - Rotas em kebab-case via IOutboundParameterTransformer
    // - URLs em lowercase
    // - JSON com camelCase, enums como string, tolerancia a input
    // - API versioning via URL segment (/api/v{version}/...) com header fallback
    //
    // O BedrockControllersOptions permite estender cada configuracao via callbacks
    // que sao invocados APOS os defaults do Bedrock.
    // Retorna IMvcBuilder para o cliente encadear filtros, etc.
    public static IMvcBuilder AddBedrockControllers(
        this IServiceCollection services,
        BedrockControllersOptions? options = null)
    {
        ConfigureRouting(services, options?.ConfigureRouting);
        ConfigureApiVersioning(services, options?.ConfigureApiVersioning, options?.ConfigureApiExplorer);

        return services
            .AddControllers(mvc =>
            {
                mvc.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
                mvc.Filters.AddService<MessageTranslationActionFilter>();
                options?.ConfigureMvc?.Invoke(mvc);
            })
            .AddJsonOptions(json =>
            {
                BedrockJsonDefaults.Configure(json.JsonSerializerOptions);
                options?.ConfigureJson?.Invoke(json.JsonSerializerOptions);
            });
    }

    // URLs em lowercase para consistencia — evita duplicidade de rotas
    // entre /Auth/Register e /auth/register que seriam tratadas como
    // endpoints distintos por proxies e caches.
    private static void ConfigureRouting(
        IServiceCollection services,
        Action<Microsoft.AspNetCore.Routing.RouteOptions>? configure)
    {
        services.AddRouting(routing =>
        {
            routing.LowercaseUrls = true;
            routing.LowercaseQueryStrings = true;
            configure?.Invoke(routing);
        });
    }

    // Configura API versioning com duas estrategias combinadas:
    // 1. URL segment (primaria): /api/v1/auth/login — explicita, visivel, cacheavel
    // 2. Header (fallback): Api-Version: 1 — para clients que preferem URL limpa
    //
    // AssumeDefaultVersionWhenUnspecified garante que requests sem versao
    // sejam roteados para a versao default em vez de retornar 400.
    // ReportApiVersions adiciona o header api-supported-versions na resposta
    // para que clients descubram versoes disponiveis.
    private static void ConfigureApiVersioning(
        IServiceCollection services,
        Action<ApiVersioningOptions>? configureVersioning,
        Action<ApiExplorerOptions>? configureExplorer)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("Api-Version")
                );
                configureVersioning?.Invoke(options);
            })
            .AddApiExplorer(options =>
            {
                // Formato: 'v'major — ex: v1, v2
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
                configureExplorer?.Invoke(options);
            });
    }
}
