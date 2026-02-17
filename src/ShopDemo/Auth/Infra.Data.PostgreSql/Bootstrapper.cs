using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShopDemo.Auth.Infra.Data.PostgreSql.Connections;
using ShopDemo.Auth.Infra.Data.PostgreSql.Connections.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.Mappers;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories;
using ShopDemo.Auth.Infra.Data.PostgreSql.Repositories.Interfaces;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork;
using ShopDemo.Auth.Infra.Data.PostgreSql.UnitOfWork.Interfaces;

namespace ShopDemo.Auth.Infra.Data.PostgreSql;

/// <summary>
/// Registra os servicos da camada Infra.Data.PostgreSql do Auth no IoC.
/// </summary>
public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Mappers (singleton — stateless, caches internamente)
        services.TryAddSingleton<IDataModelMapper<UserDataModel>, UserDataModelMapper>();
        services.TryAddSingleton<IDataModelMapper<RefreshTokenDataModel>, RefreshTokenDataModelMapper>();

        // Connection (scoped — mantém NpgsqlConnection por request)
        services.TryAddScoped<IAuthPostgreSqlConnection, AuthPostgreSqlConnection>();

        // Unit of Work (scoped — mantém transacao por request)
        services.TryAddScoped<IAuthPostgreSqlUnitOfWork, AuthPostgreSqlUnitOfWork>();

        // DataModel Repositories (scoped — dependem do UoW)
        services.TryAddScoped<IUserDataModelRepository, UserDataModelRepository>();
        services.TryAddScoped<IRefreshTokenDataModelRepository, RefreshTokenDataModelRepository>();

        // PostgreSql Repositories (scoped — dependem do DataModel Repository)
        services.TryAddScoped<IUserPostgreSqlRepository, UserPostgreSqlRepository>();
        services.TryAddScoped<IRefreshTokenPostgreSqlRepository, RefreshTokenPostgreSqlRepository>();

        return services;
    }
}
