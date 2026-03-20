using Bedrock.BuildingBlocks.Web.DataProtection.Extensions;
using Bedrock.BuildingBlocks.Web.DataProtection.Models;
using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Bedrock.BuildingBlocks.Web.GracefulShutdown.Extensions;
using Bedrock.BuildingBlocks.Web.GracefulShutdown.Models;
using Bedrock.BuildingBlocks.Web.Hosting.Models;
using Bedrock.BuildingBlocks.Web.Logging.Extensions;
using Bedrock.BuildingBlocks.Web.Logging.Models;
using Bedrock.BuildingBlocks.Web.WebApi;
using Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.Cors.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.Cors.Models;
using Bedrock.BuildingBlocks.Web.WebApi.ExceptionHandling.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.ExceptionHandling.Models;
using Bedrock.BuildingBlocks.Web.WebApi.Controllers.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;
using Bedrock.BuildingBlocks.Web.WebApi.OutputCaching.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.OutputCaching.Models;
using Bedrock.BuildingBlocks.Web.WebApi.RateLimiting.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.RateLimiting.Models;
using Bedrock.BuildingBlocks.Web.WebApi.Validation.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ShopDemo.Auth.Api.Constants;
using ShopDemo.Auth.Api.HealthChecks.Services;

namespace ShopDemo.Auth.Api;

public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services, StartupInfo startupInfo)
    {
        services.AddBedrockWebApi(startupInfo);

        services.AddBedrockExceptionHandling(new BedrockExceptionHandlingOptions());

        services.AddBedrockValidation();

        services.AddBedrockRequestLogging(new BedrockRequestLoggingOptions()
            .ExcludePaths("/health/*", "/scalar/*")
        );

        services.AddBedrockGracefulShutdown(new BedrockGracefulShutdownOptions()
            .WithTimeout(TimeSpan.FromSeconds(30))
        );

        services.AddBedrockDataProtection(new BedrockDataProtectionOptions()
            .WithApplicationName("ShopDemo.Auth")
        );

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

        services.AddBedrockCors(new BedrockCorsOptions()
            .AddPolicy(CorsPolicyNames.Default, policy => policy
                .WithOrigins("https://localhost:3000")
                .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                .WithHeaders(
                    "Content-Type",
                    "Authorization",
                    ExecutionContextFactory.CorrelationIdHeaderName,
                    ExecutionContextFactory.TenantIdHeaderName,
                    ExecutionContextFactory.ExecutionUserHeaderName,
                    ExecutionContextFactory.ExecutionOriginHeaderName,
                    ExecutionContextFactory.BusinessOperationCodeHeaderName)
                .WithExposedHeaders(
                    "Retry-After",
                    ExecutionContextFactory.CorrelationIdHeaderName,
                    ExecutionContextFactory.TenantIdHeaderName,
                    ExecutionContextFactory.ExecutionUserHeaderName,
                    ExecutionContextFactory.ExecutionOriginHeaderName,
                    ExecutionContextFactory.BusinessOperationCodeHeaderName)
                .WithCredentials()
                .WithPreflightMaxAge(TimeSpan.FromHours(1)))
            .SetDefaultPolicy(CorsPolicyNames.Default)
        );

        services.AddBedrockOutputCaching(new BedrockOutputCachingOptions()
            .WithDefaultExpiration(TimeSpan.FromMinutes(5))
        );

        services.AddBedrockControllers();
        services.AddBedrockApiDocumentation();

        return services;
    }
}
