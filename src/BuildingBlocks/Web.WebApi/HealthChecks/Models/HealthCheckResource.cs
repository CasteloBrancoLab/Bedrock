namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;

// Representa o estado de um recurso individual (banco, cache, fila, etc.)
// reportado no body do health check. O status do recurso e informativo e
// NAO influencia o resultado do health check — o probe pode ser Healthy
// mesmo que um recurso individual esteja Unhealthy.
// O Timestamp indica quando o recurso foi verificado pela ultima vez,
// permitindo que health checks usem status cacheado/assincrono sem
// precisar abrir conexoes a cada ciclo de probe.
public sealed record HealthCheckResource(
    string Name,
    HealthCheckResourceStatus Status,
    DateTimeOffset Timestamp,
    string? Description = null
);
