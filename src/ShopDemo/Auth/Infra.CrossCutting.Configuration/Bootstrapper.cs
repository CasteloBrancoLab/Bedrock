using Bedrock.BuildingBlocks.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShopDemo.Auth.Infra.CrossCutting.Configuration;

/// <summary>
/// Registra os servicos da camada Infra.CrossCutting.Configuration do Auth no IoC.
/// </summary>
public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddBedrockConfiguration<AuthConfigurationManager>();

        return services;
    }
}

