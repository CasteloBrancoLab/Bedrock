using System.Threading.RateLimiting;
using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.WebApi.RateLimiting;

public static class RateLimitingServiceCollectionExtensions
{
    // Registra as politicas de rate limiting configuradas via fluent API.
    //
    // Todas as politicas usam sliding window como algoritmo default:
    // - Distribui permits em segmentos dentro da janela (segmentsPerWindow = 4)
    // - Evita spikes no reset da janela que ocorrem com fixed window
    // - QueueLimit = 0 (rejeita imediatamente em vez de enfileirar)
    //
    // O response padrao para requests limitados e 429 Too Many Requests
    // com header Retry-After indicando quando tentar novamente.
    public static IServiceCollection AddBedrockRateLimiting(
        this IServiceCollection services,
        BedrockRateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        services.AddRateLimiter(limiter =>
        {
            ConfigureDefaults(limiter);

            if (options.Global is not null)
            {
                RegisterGlobalPolicy(limiter, options.Global);
            }

            if (options.Tenant is not null)
            {
                RegisterTenantPolicy(limiter, options.Tenant);
            }

            RegisterRoutePolicies(limiter, options.Routes);
            RegisterCompositeKeyPolicies(limiter, options.CompositeKeys);

            options.ConfigureRateLimiter?.Invoke(limiter);
        });

        return services;
    }

    private static void ConfigureDefaults(RateLimiterOptions limiter)
    {
        limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        limiter.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
            }

            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsJsonAsync(
                new { code = "TooManyRequests", message = "Rate limit exceeded. Please try again later." },
                cancellationToken);
        };
    }

    // Global: particao unica para todo o servico.
    // Todas as requests compartilham o mesmo bucket.
    private static void RegisterGlobalPolicy(
        RateLimiterOptions limiter,
        GlobalRateLimitPolicy policy)
    {
        limiter.AddSlidingWindowLimiter(BedrockRateLimitingPolicyNames.Global, sliding =>
        {
            ConfigureSlidingWindow(sliding, policy.PermitLimit, policy.Window);
            policy.Configure?.Invoke(sliding);
        });
    }

    // Tenant: particao por X-Tenant-Id header.
    // Cada tenant recebe seu proprio bucket independente.
    // Requests sem tenant recebem particao "anonymous".
    private static void RegisterTenantPolicy(
        RateLimiterOptions limiter,
        TenantRateLimitPolicy policy)
    {
        limiter.AddPolicy(BedrockRateLimitingPolicyNames.Tenant, context =>
        {
            var tenantId = context.Request.Headers[ExecutionContextFactory.TenantIdHeaderName].FirstOrDefault()
                ?? "anonymous";

            return RateLimitPartition.GetSlidingWindowLimiter(tenantId, _ =>
            {
                var options = new SlidingWindowRateLimiterOptions();
                ConfigureSlidingWindow(options, policy.PermitLimit, policy.Window);
                policy.Configure?.Invoke(options);
                return options;
            });
        });
    }

    // Route: cada politica nomeada recebe sua propria configuracao.
    // Aplicada via [EnableRateLimiting("policy-name")] na action ou controller.
    private static void RegisterRoutePolicies(
        RateLimiterOptions limiter,
        List<RouteRateLimitPolicy> policies)
    {
        foreach (var policy in policies)
        {
            limiter.AddSlidingWindowLimiter(policy.PolicyName, sliding =>
            {
                ConfigureSlidingWindow(sliding, policy.PermitLimit, policy.Window);
                policy.Configure?.Invoke(sliding);
            });
        }
    }

    // Composite key: particao por chave dinamica definida pelo consumidor.
    // Aplicada via [EnableRateLimiting("policy-name")] na action ou controller.
    private static void RegisterCompositeKeyPolicies(
        RateLimiterOptions limiter,
        List<CompositeKeyRateLimitPolicy> policies)
    {
        foreach (var policy in policies)
        {
            limiter.AddPolicy(policy.PolicyName, context =>
            {
                var partitionKey = policy.PartitionKeyFactory(context);

                return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ =>
                {
                    var options = new SlidingWindowRateLimiterOptions();
                    ConfigureSlidingWindow(options, policy.PermitLimit, policy.Window);
                    policy.Configure?.Invoke(options);
                    return options;
                });
            });
        }
    }

    // Sliding window defaults:
    // - SegmentsPerWindow = 4: divide a janela em 4 segmentos para suavizar a distribuicao.
    //   Ex: janela de 1 min = segmentos de 15s, permits expiram gradualmente.
    // - QueueLimit = 0: rejeita imediatamente requests excedentes.
    //   Em APIs, enfileirar aumenta latencia e consome memoria — melhor rejeitar rapido.
    // - AutoReplenishment = true: reposicao automatica de permits sem timer externo.
    private static void ConfigureSlidingWindow(
        SlidingWindowRateLimiterOptions options,
        int permitLimit,
        TimeSpan window)
    {
        options.PermitLimit = permitLimit;
        options.Window = window;
        options.SegmentsPerWindow = 4;
        options.QueueLimit = 0;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.AutoReplenishment = true;
    }
}
