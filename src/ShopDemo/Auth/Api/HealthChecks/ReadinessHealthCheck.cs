using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;

namespace ShopDemo.Auth.Api.HealthChecks;

public sealed class ReadinessHealthCheck : ReadinessHealthCheckBase
{
    protected override Task<HealthCheckOutput> CheckReadinessAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(HealthCheckOutput.Healthy());
    }
}
