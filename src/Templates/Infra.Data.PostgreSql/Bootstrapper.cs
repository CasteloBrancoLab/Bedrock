using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Templates.Infra.Data.PostgreSql.Connections;
using Templates.Infra.Data.PostgreSql.Connections.Interfaces;
using Templates.Infra.Data.PostgreSql.DataModels;
using Templates.Infra.Data.PostgreSql.DataModelsRepositories;
using Templates.Infra.Data.PostgreSql.DataModelsRepositories.Interfaces;
using Templates.Infra.Data.PostgreSql.Mappers;
using Templates.Infra.Data.PostgreSql.Repositories;
using Templates.Infra.Data.PostgreSql.Repositories.Interfaces;
using Templates.Infra.Data.PostgreSql.UnitOfWork;
using Templates.Infra.Data.PostgreSql.UnitOfWork.Interfaces;

namespace Templates.Infra.Data.PostgreSql;

/// <summary>
/// Registra os servicos da camada Infra.Data.PostgreSql do Templates no IoC.
/// </summary>
public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Mappers (singleton — stateless, caches internamente)
        services.TryAddSingleton<IDataModelMapper<SimpleAggregateRootDataModel>, SimpleAggregateRootDataModelMapper>();

        // Connection (scoped — mantém NpgsqlConnection por request)
        services.TryAddScoped<ITemplatesPostgreSqlConnection, TemplatesPostgreSqlConnection>();

        // Unit of Work (scoped — mantém transacao por request)
        services.TryAddScoped<ITemplatesPostgreSqlUnitOfWork, TemplatesPostgreSqlUnitOfWork>();

        // DataModel Repositories (scoped — dependem do UoW)
        services.TryAddScoped<ISimpleAggregateRootDataModelRepository, SimpleAggregateRootDataModelRepository>();

        // PostgreSql Repositories (scoped — dependem do DataModel Repository)
        services.TryAddScoped<ISimpleAggregateRootPostgreSqlRepository, SimpleAggregateRootPostgreSqlRepository>();

        return services;
    }
}
