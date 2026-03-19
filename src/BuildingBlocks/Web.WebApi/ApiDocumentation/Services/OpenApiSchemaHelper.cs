using Microsoft.OpenApi;

namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Services;

internal static class OpenApiSchemaHelper
{
    public static OpenApiSchema CreateStringSchema()
    {
        return new OpenApiSchema { Type = JsonSchemaType.String };
    }
}
