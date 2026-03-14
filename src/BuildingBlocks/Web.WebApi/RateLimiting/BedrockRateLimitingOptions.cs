using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Bedrock.BuildingBlocks.Web.WebApi.RateLimiting;

// Configuracao fluente para as politicas de rate limiting do Bedrock.
//
// Quatro camadas de protecao, cada uma opcional e independente:
// 1. Global — protege o servico contra overload total
// 2. Per-tenant — previne noisy neighbor via X-Tenant-Id
// 3. Per-route — limites especificos por endpoint (ex: login, register)
// 4. Composite key — chave dinamica composta pelo consumidor
//
// Todas usam sliding window como default (distribuicao mais suave que fixed window,
// evita spikes no reset da janela). O consumidor pode sobrescrever via callback.
public sealed class BedrockRateLimitingOptions
{
    internal GlobalRateLimitPolicy? Global { get; private set; }
    internal TenantRateLimitPolicy? Tenant { get; private set; }
    internal List<RouteRateLimitPolicy> Routes { get; } = [];
    internal List<CompositeKeyRateLimitPolicy> CompositeKeys { get; } = [];
    internal Action<RateLimiterOptions>? ConfigureRateLimiter { get; private set; }

    // Limita o numero total de requests para o servico inteiro.
    // Protege contra overload independente de quem esta chamando.
    public BedrockRateLimitingOptions AddGlobalPolicy(
        int permitLimit,
        TimeSpan window,
        Action<SlidingWindowRateLimiterOptions>? configure = null)
    {
        Global = new GlobalRateLimitPolicy(permitLimit, window, configure);
        return this;
    }

    // Limita requests por tenant usando o header X-Tenant-Id.
    // Previne que um tenant consuma toda a capacidade do servico (noisy neighbor).
    // Requests sem X-Tenant-Id recebem uma particao propria ("anonymous").
    public BedrockRateLimitingOptions AddTenantPolicy(
        int permitLimit,
        TimeSpan window,
        Action<SlidingWindowRateLimiterOptions>? configure = null)
    {
        Tenant = new TenantRateLimitPolicy(permitLimit, window, configure);
        return this;
    }

    // Limita requests para uma rota especifica (policy name usado com [EnableRateLimiting]).
    // Util para endpoints sensiveis como login (brute force) ou register (spam).
    //
    // Uso na controller:
    //   [EnableRateLimiting("login")]
    //   public async Task<IActionResult> Login(...)
    public BedrockRateLimitingOptions AddRoutePolicy(
        string policyName,
        int permitLimit,
        TimeSpan window,
        Action<SlidingWindowRateLimiterOptions>? configure = null)
    {
        Routes.Add(new RouteRateLimitPolicy(policyName, permitLimit, window, configure));
        return this;
    }

    // Limita requests por uma chave composta definida dinamicamente pelo consumidor.
    // A funcao partitionKeyFactory recebe o HttpContext e retorna a chave de particao.
    //
    // Exemplo: tenant + user + rota
    //   .AddCompositeKeyPolicy("tenant-user-route", 50, TimeSpan.FromMinutes(1),
    //       context => $"{context.Request.Headers["X-Tenant-Id"]}" +
    //                  $":{context.Request.Headers["X-Execution-User"]}" +
    //                  $":{context.Request.Path}")
    public BedrockRateLimitingOptions AddCompositeKeyPolicy(
        string policyName,
        int permitLimit,
        TimeSpan window,
        Func<HttpContext, string> partitionKeyFactory,
        Action<SlidingWindowRateLimiterOptions>? configure = null)
    {
        CompositeKeys.Add(new CompositeKeyRateLimitPolicy(policyName, permitLimit, window, partitionKeyFactory, configure));
        return this;
    }

    // Callback para estender ou sobrescrever a configuracao do RateLimiterOptions
    // apos os defaults do Bedrock (ex: customizar RejectionStatusCode, OnRejected).
    public BedrockRateLimitingOptions Configure(Action<RateLimiterOptions> configure)
    {
        ConfigureRateLimiter = configure;
        return this;
    }
}
