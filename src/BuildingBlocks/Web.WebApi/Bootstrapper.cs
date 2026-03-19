using Bedrock.BuildingBlocks.Web.Hosting.Models;
using Bedrock.BuildingBlocks.Web.WebApi.Envelope.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.WebApi;

public static class Bootstrapper
{
    public static IServiceCollection AddBedrockWebApi(this IServiceCollection services, StartupInfo startupInfo)
    {
        services.AddBedrockWeb(startupInfo);
        services.AddBedrockEnvelope();

        return services;
    }
}
