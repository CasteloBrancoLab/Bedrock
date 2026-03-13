namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;

// Status possivel de um recurso individual no health check.
public enum HealthCheckResourceStatus
{
    Healthy,
    Degraded,
    Unhealthy
}
