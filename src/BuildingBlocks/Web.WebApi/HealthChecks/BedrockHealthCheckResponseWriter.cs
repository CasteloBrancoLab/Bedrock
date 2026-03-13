using System.Text.Json;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;
using Bedrock.BuildingBlocks.Web.WebApi.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks;

// Serializa a resposta do health check em JSON com o status geral,
// timestamp e array de recursos informativos. O formato e pensado
// para ser consumido por dashboards, Kubernetes e ferramentas de monitoramento.
//
// Exemplo de resposta:
// {
//   "status": "healthy",
//   "description": null,
//   "timestamp": "2026-03-13T17:00:00.000Z",
//   "resources": [
//     { "name": "PostgreSQL", "status": "unhealthy", "timestamp": "2026-03-13T16:55:00Z", "description": "Connection timeout" },
//     { "name": "Redis", "status": "healthy", "timestamp": "2026-03-13T16:59:30Z", "description": null }
//   ]
// }
internal static class BedrockHealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public static Task WriteAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json";

        // Um health check report pode ter multiplas entries, mas no Bedrock
        // cada probe tem exatamente uma entry. Pegamos a primeira.
        var entry = report.Entries.Values.FirstOrDefault();

        var resources = entry.Data.TryGetValue(BedrockHealthCheckBase.ResourcesDataKey, out var resourcesObj)
            ? resourcesObj as IReadOnlyList<HealthCheckResource> ?? []
            : [];

        var response = new HealthCheckResponse(
            Status: report.Status.ToString(),
            Description: entry.Description,
            Timestamp: DateTimeOffset.UtcNow,
            Resources: resources
        );

        return httpContext.Response.WriteAsJsonAsync(response, JsonOptions);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = false };
        BedrockJsonDefaults.Configure(options);
        return options;
    }

    private sealed record HealthCheckResponse(
        string Status,
        string? Description,
        DateTimeOffset Timestamp,
        IReadOnlyList<HealthCheckResource> Resources
    );
}
