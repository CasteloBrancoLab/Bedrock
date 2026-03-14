using Bedrock.BuildingBlocks.Web.Hosting;
using Bedrock.BuildingBlocks.Web.WebApi;
using Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation;
using Bedrock.BuildingBlocks.Web.WebApi.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks;
using Bedrock.BuildingBlocks.Web.WebApi.RateLimiting;
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

        services.AddBedrockRateLimiting(new BedrockRateLimitingOptions()
            .AddGlobalPolicy(permitLimit: 1000, window: TimeSpan.FromMinutes(1))
            .AddTenantPolicy(permitLimit: 200, window: TimeSpan.FromMinutes(1))
            .AddRoutePolicy(RateLimitPolicyNames.Login, permitLimit: 10, window: TimeSpan.FromMinutes(1))
            .AddRoutePolicy(RateLimitPolicyNames.Register, permitLimit: 5, window: TimeSpan.FromMinutes(1))
        );

        services.AddBedrockControllers();
        services.AddBedrockApiDocumentation();

        return services;
    }
}
