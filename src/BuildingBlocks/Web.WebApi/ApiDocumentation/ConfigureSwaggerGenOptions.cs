using Asp.Versioning.ApiExplorer;
using Bedrock.BuildingBlocks.Web.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation;

// Registra automaticamente um SwaggerDoc por versao descoberta pelo ApiExplorer.
// Executado em tempo de resolucao (nao em ConfigureServices), garantindo que
// todas as versoes ja tenham sido registradas pelo Asp.Versioning antes de iterar.
//
// Usa StartupInfo para preencher titulo e versao do assembly,
// evitando duplicacao de metadados entre o projeto e a spec OpenAPI.
internal sealed class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly StartupInfo _startupInfo;

    public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider provider, StartupInfo startupInfo)
    {
        _provider = provider;
        _startupInfo = startupInfo;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateOpenApiInfo(description));
        }
    }

    private OpenApiInfo CreateOpenApiInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = _startupInfo.AssemblyName,
            Version = description.ApiVersion.ToString(),
        };

        if (description.IsDeprecated)
        {
            info.Description = "This API version has been deprecated.";
        }

        return info;
    }
}
