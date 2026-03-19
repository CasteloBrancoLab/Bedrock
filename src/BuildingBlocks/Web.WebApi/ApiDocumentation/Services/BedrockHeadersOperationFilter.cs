using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Services;

// Adiciona automaticamente os headers do ExecutionContext como parametros
// opcionais em todas as operations da spec OpenAPI.
//
// Headers excluidos via [ExcludeBedrockHeader] ou [ExcludeAllBedrockHeaders]
// nao sao adicionados.
internal sealed class BedrockHeadersOperationFilter : IOperationFilter
{
    private static readonly BedrockHeaderDefinition[] Headers =
    [
        new(ExecutionContextFactory.CorrelationIdHeaderName, "Correlation ID for distributed tracing. Auto-generated if not provided."),
        new(ExecutionContextFactory.TenantIdHeaderName, "Tenant identifier for multi-tenant operations."),
        new(ExecutionContextFactory.ExecutionUserHeaderName, "User identity extracted from JWT by the API gateway."),
        new(ExecutionContextFactory.ExecutionOriginHeaderName, "Origin of the request (e.g., web, mobile, service)."),
        new(ExecutionContextFactory.BusinessOperationCodeHeaderName, "Business operation code propagated by the initiator service for end-to-end use case observability."),
        new("Authorization", "JWT forwarded by the API gateway for additional claims extraction. Services must not validate signature or expiration."),
    ];

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (ShouldExcludeAll(context))
        {
            return;
        }

        var excludedNames = GetExcludedHeaderNames(context);

        foreach (var header in Headers)
        {
            if (excludedNames.Contains(header.Name, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            AddHeaderParameter(operation, header);
        }
    }

    private static bool ShouldExcludeAll(OperationFilterContext context)
    {
        return HasAttribute<ExcludeAllBedrockHeadersAttribute>(context);
    }

    private static HashSet<string> GetExcludedHeaderNames(OperationFilterContext context)
    {
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        CollectExcludedHeaders(context.MethodInfo.GetCustomAttributes(typeof(ExcludeBedrockHeaderAttribute), false), excluded);
        CollectExcludedHeaders(context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(ExcludeBedrockHeaderAttribute), false), excluded);

        return excluded;
    }

    private static void CollectExcludedHeaders(object[]? attributes, HashSet<string> excluded)
    {
        if (attributes is null)
        {
            return;
        }

        foreach (var attr in attributes)
        {
            if (attr is ExcludeBedrockHeaderAttribute exclude)
            {
                foreach (var name in exclude.HeaderNames)
                {
                    excluded.Add(name);
                }
            }
        }
    }

    private static bool HasAttribute<T>(OperationFilterContext context) where T : Attribute
    {
        return context.MethodInfo.GetCustomAttributes(typeof(T), false).Length > 0
            || (context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(T), false).Length > 0);
    }

    private static void AddHeaderParameter(OpenApiOperation operation, BedrockHeaderDefinition header)
    {
        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = header.Name,
            In = ParameterLocation.Header,
            Required = false,
            Description = header.Description,
            Schema = OpenApiSchemaHelper.CreateStringSchema(),
        });
    }

    private sealed record BedrockHeaderDefinition(string Name, string Description);
}
