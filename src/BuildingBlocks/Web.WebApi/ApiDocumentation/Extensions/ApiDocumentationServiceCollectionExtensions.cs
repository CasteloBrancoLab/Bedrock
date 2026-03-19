using Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Models;
using Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Extensions;

public static class ApiDocumentationServiceCollectionExtensions
{
    // Registra Swashbuckle para geracao da spec OpenAPI com suporte a API versioning.
    public static IServiceCollection AddBedrockApiDocumentation(
        this IServiceCollection services,
        BedrockApiDocumentationOptions? options = null)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();

        services.AddSwaggerGen(swagger =>
        {
            swagger.OperationFilter<BedrockHeadersOperationFilter>();
            swagger.AddScalarFilters();
            options?.ConfigureSwaggerGen?.Invoke(swagger);
        });

        return services;
    }
}
