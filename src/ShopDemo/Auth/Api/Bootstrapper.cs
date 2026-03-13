using Bedrock.BuildingBlocks.Web.Hosting;
using Bedrock.BuildingBlocks.Web.WebApi;
using Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation;
using Bedrock.BuildingBlocks.Web.WebApi.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using ShopDemo.Auth.Api.HealthChecks;

namespace ShopDemo.Auth.Api;

public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services, StartupInfo startupInfo)
    {
        services.AddBedrockWebApi(startupInfo);

        services.AddBedrockHealthChecks(new BedrockHealthCheckOptions()
            .AddStartupCheck<StartupHealthCheck>("/health/startup")
            .AddReadinessCheck<ReadinessHealthCheck>("/health/ready")
            .AddLivenessCheck<LivenessHealthCheck>("/health/live")
        );

        services.AddBedrockControllers();
        services.AddBedrockApiDocumentation();

        return services;
    }
}
