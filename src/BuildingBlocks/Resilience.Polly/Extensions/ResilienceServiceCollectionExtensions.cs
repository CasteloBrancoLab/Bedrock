using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bedrock.BuildingBlocks.Resilience.Polly.Extensions;

/// <summary>
/// Extension methods for registering Polly-based resilience policies in the DI container.
/// </summary>
public static class ResilienceServiceCollectionExtensions
{
    /// <summary>
    /// Registers a Polly-based resilience policy as a singleton.
    /// Also registers it as <see cref="IResiliencePolicy"/> for automatic discovery
    /// by the <see cref="IResiliencePolicyManager"/>.
    /// </summary>
    /// <typeparam name="TPolicy">The concrete resilience policy type.</typeparam>
    public static IServiceCollection AddBedrockResiliencePolicy<TPolicy>(this IServiceCollection services)
        where TPolicy : PollyResiliencePolicyBase
    {
        services.TryAddSingleton<TPolicy>();

        // Register as IResiliencePolicy for Manager discovery via GetServices<IResiliencePolicy>()
        services.AddSingleton<IResiliencePolicy>(sp => sp.GetRequiredService<TPolicy>());

        return services;
    }
}
