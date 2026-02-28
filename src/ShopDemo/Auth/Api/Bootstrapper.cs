using Microsoft.Extensions.DependencyInjection;

namespace ShopDemo.Auth.Api;

public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(typeof(Bootstrapper).Assembly);

        return services;
    }
}
