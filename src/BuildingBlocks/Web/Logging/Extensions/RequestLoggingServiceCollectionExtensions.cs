using Bedrock.BuildingBlocks.Web.Logging.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.Logging.Extensions;

public static class RequestLoggingServiceCollectionExtensions
{
    // Registra o middleware de request logging com as options configuradas.
    public static IServiceCollection AddBedrockRequestLogging(
        this IServiceCollection services,
        BedrockRequestLoggingOptions? options = null)
    {
        services.AddSingleton(options ?? new BedrockRequestLoggingOptions());
        return services;
    }
}
