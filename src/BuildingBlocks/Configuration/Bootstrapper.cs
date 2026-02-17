using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bedrock.BuildingBlocks.Configuration;

/// <summary>
/// Registra o Configuration BuildingBlock no IoC.
/// </summary>
public static class Bootstrapper
{
    /// <summary>
    /// Registra um ConfigurationManager concreto e seus handlers no container de DI.
    /// </summary>
    /// <typeparam name="TManager">Tipo concreto do ConfigurationManager.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Action para configurar opcoes (secoes, handlers, pipeline).</param>
    /// <returns>Service collection para encadeamento.</returns>
    public static IServiceCollection AddBedrockConfiguration<TManager>(
        this IServiceCollection services,
        Action<ConfigurationOptions>? configure = null)
        where TManager : ConfigurationManagerBase
    {
        services.TryAddSingleton<TManager>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<TManager>();

            return (TManager)Activator.CreateInstance(typeof(TManager), configuration, logger)!;
        });

        return services;
    }
}
