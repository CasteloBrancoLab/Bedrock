using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Services;

// Classe base interna que implementa IHealthCheck e converte o HealthCheckOutput
// (com recursos informativos) em HealthCheckResult do ASP.NET Core.
// Os recursos sao armazenados no dicionario Data do HealthCheckResult para
// que o response writer possa serializa-los no body da resposta.
public abstract class BedrockHealthCheckBase : IHealthCheck
{
    internal const string ResourcesDataKey = "resources";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var output = await CheckHealthInternalAsync(cancellationToken);

        var data = new Dictionary<string, object>();

        if (output.Resources.Count > 0)
        {
            data[ResourcesDataKey] = output.Resources;
        }

        return new HealthCheckResult(
            status: output.Status,
            description: output.Description,
            data: data);
    }

    protected abstract Task<HealthCheckOutput> CheckHealthInternalAsync(CancellationToken cancellationToken);
}
