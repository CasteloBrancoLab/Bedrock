using Bedrock.BuildingBlocks.Web.Hosting;
using Bedrock.BuildingBlocks.Web.Hosting.Extensions;
using Bedrock.BuildingBlocks.Web.Hosting.Models;
using Bedrock.BuildingBlocks.Web.Logging.Extensions;
using Bedrock.BuildingBlocks.Web.Security.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.Cors.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.CorrelationId.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.ExceptionHandling.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.OutputCaching.Extensions;
using Bedrock.BuildingBlocks.Web.WebApi.RateLimiting.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ShopDemo.Auth.Api;

namespace ShopDemo.Auth.Api;

public class Program
{
    public static void Main(string[] args)
    {
        BedrockHost.ConfigureDefaults(out var startupInfo);

        var builder = WebApplication.CreateBuilder(args);

        builder.UseBedrockKestrel(new BedrockKestrelOptions()
            .Listen(5080, HttpProtocols.Http1)
            .Listen(5081, HttpProtocols.Http2)
        );

        Bootstrapper.ConfigureServices(builder.Services, startupInfo);

        var app = builder.Build();

        app.UseBedrockSecurityHeaders();
        app.UseBedrockCorrelationId();
        app.UseBedrockExceptionHandling();
        app.UseBedrockRequestLogging();
        app.UseBedrockRateLimiting();
        app.UseBedrockCors();
        app.UseBedrockOutputCaching();
        app.UseBedrockApiDocumentation();
        app.UseAuthorization();
        app.MapControllers();
        app.MapBedrockHealthChecks();

        app.Run();
    }
}
