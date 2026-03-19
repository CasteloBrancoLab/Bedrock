using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Models;

// Callbacks opcionais para estender a configuracao padrao de documentacao da API.
// Cada callback e invocado APOS os defaults do Bedrock, permitindo que o cliente
// sobreponha ou complemente qualquer configuracao sem perder os defaults.
public sealed class BedrockApiDocumentationOptions
{
    public Action<SwaggerGenOptions>? ConfigureSwaggerGen { get; set; }
    public Action<ScalarOptions>? ConfigureScalar { get; set; }
}
