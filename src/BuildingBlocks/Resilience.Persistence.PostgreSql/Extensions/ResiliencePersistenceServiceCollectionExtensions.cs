using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bedrock.BuildingBlocks.Resilience.Persistence.PostgreSql.Extensions;

/// <summary>
/// Extension methods for registering PostgreSQL-backed circuit breaker state stores in the DI container.
/// </summary>
public static class ResiliencePersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers a PostgreSQL-backed circuit breaker state store as a singleton.
    /// </summary>
    /// <typeparam name="TStore">The concrete state store type inheriting from <see cref="CircuitBreakerStateStoreBase"/>.</typeparam>
    public static IServiceCollection AddBedrockCircuitBreakerStateStore<TStore>(this IServiceCollection services)
        where TStore : CircuitBreakerStateStoreBase
    {
        services.TryAddSingleton<ICircuitBreakerStateStore, TStore>();
        return services;
    }
}
