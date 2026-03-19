using System.Text.Json;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;
using Bedrock.BuildingBlocks.Web.WebApi.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Services;

// Serializa a resposta do health check em JSON com o status geral,
// timestamp e array de recursos informativos. O formato e pensado
// para ser consumido por dashboards, Kubernetes e ferramentas de monitoramento.
internal static class BedrockHealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public static Task WriteAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json";

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
