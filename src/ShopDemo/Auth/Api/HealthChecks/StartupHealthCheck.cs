using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks;
using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;

namespace ShopDemo.Auth.Api.HealthChecks;

public sealed class StartupHealthCheck : StartupHealthCheckBase
{
    protected override Task<HealthCheckOutput> CheckStartupAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(HealthCheckOutput.Healthy());
    }
}
