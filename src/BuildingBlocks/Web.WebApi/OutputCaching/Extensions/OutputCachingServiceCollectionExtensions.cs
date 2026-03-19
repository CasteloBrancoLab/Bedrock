using Bedrock.BuildingBlocks.Web.WebApi.OutputCaching.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.WebApi.OutputCaching.Extensions;

public static class OutputCachingServiceCollectionExtensions
{
    // Registra o output caching com as politicas configuradas via fluent API.
    //
    // Cada policy nomeada pode ser referenciada nas actions via
    // [OutputCache(PolicyName = "...")] para caching seletivo.
    public static IServiceCollection AddBedrockOutputCaching(
        this IServiceCollection services,
        BedrockOutputCachingOptions? options = null)
    {
        var resolvedOptions = options ?? new BedrockOutputCachingOptions();

        services.AddOutputCache(cacheOptions =>
        {
            cacheOptions.DefaultExpirationTimeSpan = resolvedOptions.DefaultExpiration;

            foreach (var policy in resolvedOptions.Policies)
            {
                RegisterPolicy(cacheOptions, policy);
            }

            resolvedOptions.ConfigureOutputCache?.Invoke(cacheOptions);
        });

        return services;
    }

    private static void RegisterPolicy(
        Microsoft.AspNetCore.OutputCaching.OutputCacheOptions cacheOptions,
        OutputCachePolicy policy)
    {
        if (policy.Configure is not null)
        {
            cacheOptions.AddPolicy(policy.Name, policy.Configure);
            return;
        }

        cacheOptions.AddPolicy(policy.Name, builder =>
        {
            if (policy.Duration.HasValue)
            {
                builder.Expire(policy.Duration.Value);
            }

            if (policy.VaryByQuery)
            {
                builder.SetVaryByQuery("*");
            }
        });
    }
}
