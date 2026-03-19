using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Bedrock.BuildingBlocks.Web.WebApi.Resilience.Models;

// Configuracao fluente para resilience do Bedrock usando Polly v8.
//
// Define pipelines de resilience para HttpClients nomeados:
// - Standard: retry 3x exponencial, circuit breaker, timeout 30s
// - Custom: controle total via callback do HttpStandardResilienceOptions
//
// Usa Microsoft.Extensions.Http.Resilience que integra nativamente
// com IHttpClientFactory e Polly v8.
//
// Uso tipico:
//   new BedrockResilienceOptions()
//       .AddStandardPipeline("catalog-api")
//       .AddStandardPipeline("payment-api")
//       .AddCustomPipeline("notification-api", options => { options.Retry.MaxRetryAttempts = 5; })
public sealed class BedrockResilienceOptions
{
    internal List<ResiliencePipeline> Pipelines { get; } = [];
    internal Action<IHttpClientBuilder>? ConfigureHttpClient { get; private set; }

    // Adiciona um pipeline standard de resilience para um HttpClient nomeado.
    // Standard inclui: retry 3x exponencial (1s, 2s, 4s), circuit breaker, timeout 30s.
    public BedrockResilienceOptions AddStandardPipeline(string httpClientName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(httpClientName);
        Pipelines.Add(new ResiliencePipeline(httpClientName, IsStandard: true, Configure: null));
        return this;
    }

    // Adiciona um pipeline custom de resilience com controle total
    // via callback do HttpStandardResilienceOptions.
    // Permite ajustar retry, circuit breaker, timeout, etc.
    public BedrockResilienceOptions AddCustomPipeline(
        string httpClientName,
        Action<HttpStandardResilienceOptions> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(httpClientName);
        ArgumentNullException.ThrowIfNull(configure);
        Pipelines.Add(new ResiliencePipeline(httpClientName, IsStandard: false, Configure: configure));
        return this;
    }

    // Callback para estender a configuracao do IHttpClientBuilder
    // apos os defaults do Bedrock.
    public BedrockResilienceOptions Configure(Action<IHttpClientBuilder> configure)
    {
        ConfigureHttpClient = configure;
        return this;
    }
}
