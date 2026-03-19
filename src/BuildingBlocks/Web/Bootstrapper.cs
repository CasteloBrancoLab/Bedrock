using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Bedrock.BuildingBlocks.Web.ExecutionContexts.Interfaces;
using Bedrock.BuildingBlocks.Web.Hosting.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bedrock.BuildingBlocks.Web;

public static class Bootstrapper
{
    public static IServiceCollection AddBedrockWeb(this IServiceCollection services, StartupInfo startupInfo)
    {
        ArgumentNullException.ThrowIfNull(startupInfo);

        services.TryAddSingleton(startupInfo);
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.TryAddScoped<IExecutionContextFactory, ExecutionContextFactory>();

        return services;
    }
}
