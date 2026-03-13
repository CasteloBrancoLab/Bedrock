using System.Text.Json;
using System.Text.Json.Serialization;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks;

// Serializa a resposta do health check em JSON com o status geral,
// timestamp e array de recursos informativos. O formato e pensado
// para ser consumido por dashboards, Kubernetes e ferramentas de monitoramento.
//
// Exemplo de resposta:
// {
//   "status": "Healthy",
//   "description": null,
//   "timestamp": "2026-03-13T17:00:00.000Z",
//   "resources": [
//     { "name": "PostgreSQL", "status": "Unhealthy", "description": "Connection timeout" },
//     { "name": "Redis", "status": "Healthy", "description": null }
//   ]
// }
internal static class BedrockHealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = false
    };

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

    private sealed record HealthCheckResponse(
        string Status,
        string? Description,
        DateTimeOffset Timestamp,
        IReadOnlyList<HealthCheckResource> Resources
    );
}
