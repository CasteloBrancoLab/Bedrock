using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Services;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;

namespace ShopDemo.Auth.Api.HealthChecks.Services;

public sealed class ReadinessHealthCheck : ReadinessHealthCheckBase
{
    protected override Task<HealthCheckOutput> CheckReadinessAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(HealthCheckOutput.Healthy());
    }
}
