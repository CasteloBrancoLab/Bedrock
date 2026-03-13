using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;

// Resultado interno do health check retornado pelo metodo protegido que
// o filho implementa. Contem o status geral do probe e uma lista opcional
// de recursos com seus status individuais (informativos, sem influenciar o probe).
public sealed class HealthCheckOutput
{
    public HealthStatus Status { get; }
    public string? Description { get; }

    private readonly List<HealthCheckResource> _resources = [];
    public IReadOnlyList<HealthCheckResource> Resources => _resources;

    private HealthCheckOutput(HealthStatus status, string? description)
    {
        Status = status;
        Description = description;
    }

    public static HealthCheckOutput Healthy(string? description = null)
    {
        return new HealthCheckOutput(HealthStatus.Healthy, description);
    }

    public static HealthCheckOutput Degraded(string? description = null)
    {
        return new HealthCheckOutput(HealthStatus.Degraded, description);
    }

    public static HealthCheckOutput Unhealthy(string? description = null)
    {
        return new HealthCheckOutput(HealthStatus.Unhealthy, description);
    }

    public HealthCheckOutput AddResource(
        string name,
        HealthCheckResourceStatus status,
        DateTimeOffset timestamp,
        string? description = null)
    {
        _resources.Add(new HealthCheckResource(name, status, timestamp, description));
        return this;
    }
}
