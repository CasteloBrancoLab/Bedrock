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
    /// The policy is also registered as <see cref="IResiliencePolicy"/> if no other implementation exists.
    /// </summary>
    /// <typeparam name="TPolicy">The concrete resilience policy type.</typeparam>
    public static IServiceCollection AddBedrockResiliencePolicy<TPolicy>(this IServiceCollection services)
        where TPolicy : PollyResiliencePolicyBase
    {
        services.TryAddSingleton<TPolicy>();
        return services;
    }
}
