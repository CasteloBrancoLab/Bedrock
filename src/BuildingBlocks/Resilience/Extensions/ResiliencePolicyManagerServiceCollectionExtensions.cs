using Bedrock.BuildingBlocks.Resilience.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bedrock.BuildingBlocks.Resilience.Extensions;

/// <summary>
/// Extension methods for registering the <see cref="IResiliencePolicyManager"/> in the DI container.
/// </summary>
public static class ResiliencePolicyManagerServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="IResiliencePolicyManager"/> as a singleton.
    /// Discovers all <see cref="IResiliencePolicy"/> instances via DI, registers state change callbacks,
    /// and starts the background polling task for distributed state synchronization.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The manager configuration.</param>
    public static IServiceCollection AddBedrockResiliencePolicyManager(
        this IServiceCollection services,
        ResiliencePolicyManagerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton<IResiliencePolicyManager>(sp =>
        {
            var manager = new ResiliencePolicyManager(
                sp.GetRequiredService<ILogger<ResiliencePolicyManager>>(),
                sp.GetService<ICircuitBreakerStateStore>(),
                options,
                sp.GetService<TimeProvider>() ?? TimeProvider.System);

            foreach (var policy in sp.GetServices<IResiliencePolicy>())
            {
                manager.Register(policy);
            }

            manager.StartPolling();
            return manager;
        });

        return services;
    }
}
