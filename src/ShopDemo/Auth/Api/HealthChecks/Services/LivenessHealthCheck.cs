using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Services;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;

namespace ShopDemo.Auth.Api.HealthChecks.Services;

public sealed class LivenessHealthCheck : LivenessHealthCheckBase
{
    protected override Task<HealthCheckOutput> CheckLivenessAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(HealthCheckOutput.Healthy());
    }
}
