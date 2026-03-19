using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Extensions;

public static class HealthCheckServiceCollectionExtensions
{
    // Registra os health checks do Bedrock no DI. Cada probe e adicionado
    // com uma tag correspondente (startup, readiness, liveness) para que
    // o MapBedrockHealthChecks consiga filtrar por tag ao mapear os endpoints.
    public static IServiceCollection AddBedrockHealthChecks(
        this IServiceCollection services,
        BedrockHealthCheckOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);

        var builder = services.AddHealthChecks();

        if (options.StartupCheckType is not null)
        {
            RegisterCheck(builder, options.StartupCheckType, BedrockHealthCheckOptions.StartupTag);
        }

        if (options.ReadinessCheckType is not null)
        {
            RegisterCheck(builder, options.ReadinessCheckType, BedrockHealthCheckOptions.ReadinessTag);
        }

        if (options.LivenessCheckType is not null)
        {
            RegisterCheck(builder, options.LivenessCheckType, BedrockHealthCheckOptions.LivenessTag);
        }

        return services;
    }

    private static void RegisterCheck(IHealthChecksBuilder builder, Type checkType, string tag)
    {
        builder.Add(new HealthCheckRegistration(
            name: checkType.Name,
            factory: sp => (IHealthCheck)ActivatorUtilities.CreateInstance(sp, checkType),
            failureStatus: HealthStatus.Unhealthy,
            tags: [tag]));
    }
}
