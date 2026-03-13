using Bedrock.BuildingBlocks.Web.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.WebApi;

public static class Bootstrapper
{
    public static IServiceCollection AddBedrockWebApi(this IServiceCollection services, StartupInfo startupInfo)
    {
        services.AddBedrockWeb(startupInfo);

        return services;
    }
}
