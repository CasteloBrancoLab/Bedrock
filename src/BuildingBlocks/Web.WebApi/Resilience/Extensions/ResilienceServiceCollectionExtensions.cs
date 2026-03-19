using Bedrock.BuildingBlocks.Web.WebApi.Resilience.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Bedrock.BuildingBlocks.Web.WebApi.Resilience.Extensions;

public static class ResilienceServiceCollectionExtensions
{
    // Registra pipelines de resilience para HttpClients nomeados.
    //
    // Standard pipeline configura:
    // - Retry: 3 tentativas com backoff exponencial (1s, 2s, 4s)
    // - Circuit Breaker: abre apos 10 falhas em 30s, half-open apos 15s
    // - Total Request Timeout: 30s
    //
    // Usa Microsoft.Extensions.Http.Resilience que integra com Polly v8
    // e IHttpClientFactory nativamente.
    public static IServiceCollection AddBedrockResilience(
        this IServiceCollection services,
        BedrockResilienceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        foreach (var pipeline in options.Pipelines)
        {
            RegisterPipeline(services, pipeline, options);
        }

        return services;
    }

    private static void RegisterPipeline(
        IServiceCollection services,
        ResiliencePipeline pipeline,
        BedrockResilienceOptions options)
    {
        var httpClientBuilder = services.AddHttpClient(pipeline.HttpClientName);

        if (pipeline.IsStandard)
        {
            httpClientBuilder.AddStandardResilienceHandler();
        }
        else if (pipeline.Configure is not null)
        {
            httpClientBuilder.AddStandardResilienceHandler(pipeline.Configure);
        }

        options.ConfigureHttpClient?.Invoke(httpClientBuilder);
    }
}
