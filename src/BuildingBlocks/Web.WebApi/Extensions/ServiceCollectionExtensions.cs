using Bedrock.BuildingBlocks.Web.WebApi.Conventions;
using Bedrock.BuildingBlocks.Web.WebApi.Serialization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    // Registra controllers com as conventions padrao do Bedrock:
    // - Rotas em kebab-case via IOutboundParameterTransformer
    // - URLs em lowercase
    // - JSON com camelCase, enums como string, tolerancia a input
    //
    // Retorna IMvcBuilder para o cliente encadear AddApplicationPart, filtros, etc.
    public static IMvcBuilder AddBedrockControllers(this IServiceCollection services)
    {
        services.AddRouting(routing =>
        {
            routing.LowercaseUrls = true;
            routing.LowercaseQueryStrings = true;
        });

        return services
            .AddControllers(mvc =>
            {
                mvc.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
            })
            .AddJsonOptions(json =>
            {
                BedrockJsonDefaults.Configure(json.JsonSerializerOptions);
            });
    }
}
