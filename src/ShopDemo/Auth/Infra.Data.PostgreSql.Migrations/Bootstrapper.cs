using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Migrations;

/// <summary>
/// Registra os servicos da camada Infra.Data.PostgreSql.Migrations do Auth no IoC.
/// </summary>
public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton<AuthMigrationManager>();
        return services;
    }
}
