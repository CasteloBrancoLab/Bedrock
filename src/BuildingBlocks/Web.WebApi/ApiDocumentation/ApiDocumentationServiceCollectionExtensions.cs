using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation;

public static class ApiDocumentationServiceCollectionExtensions
{
    // Registra Swashbuckle para geracao da spec OpenAPI com suporte a API versioning.
    //
    // Usa IConfigureOptions<SwaggerGenOptions> (ConfigureSwaggerGenOptions) para
    // descobrir versoes automaticamente via IApiVersionDescriptionProvider em tempo
    // de resolucao, evitando a necessidade de declarar SwaggerDoc manualmente por versao.
    //
    // AddScalarFilters registra os IOperationFilter/IDocumentFilter do Scalar
    // que enriquecem a spec com extensoes proprietarias (code samples, etc.).
    public static IServiceCollection AddBedrockApiDocumentation(
        this IServiceCollection services,
        BedrockApiDocumentationOptions? options = null)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();

        services.AddSwaggerGen(swagger =>
        {
            swagger.AddScalarFilters();
            options?.ConfigureSwaggerGen?.Invoke(swagger);
        });

        return services;
    }
}
