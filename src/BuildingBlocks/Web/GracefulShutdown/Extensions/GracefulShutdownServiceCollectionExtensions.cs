using Bedrock.BuildingBlocks.Web.GracefulShutdown.Models;
using Bedrock.BuildingBlocks.Web.GracefulShutdown.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.GracefulShutdown.Extensions;

public static class GracefulShutdownServiceCollectionExtensions
{
    // Registra o servico de graceful shutdown com os callbacks configurados.
    // Configura o HostOptions.ShutdownTimeout com o valor definido nas options.
    public static IServiceCollection AddBedrockGracefulShutdown(
        this IServiceCollection services,
        BedrockGracefulShutdownOptions? options = null)
    {
        var resolvedOptions = options ?? new BedrockGracefulShutdownOptions();

        services.AddSingleton(resolvedOptions);
        services.AddHostedService<GracefulShutdownHostedService>();

        services.Configure<Microsoft.Extensions.Hosting.HostOptions>(hostOptions =>
        {
            hostOptions.ShutdownTimeout = resolvedOptions.Timeout;
        });

        return services;
    }
}
