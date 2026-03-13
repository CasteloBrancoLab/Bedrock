using Bedrock.BuildingBlocks.Web.Hosting;
using Bedrock.BuildingBlocks.Web.Security;
using Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks;
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
        app.UseBedrockApiDocumentation();
        app.UseAuthorization();
        app.MapControllers();
        app.MapBedrockHealthChecks();

        app.Run();
    }
}
