using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks;

// Base para health checks de readiness. Verifica se a aplicacao esta pronta
// para receber trafego (dependencias externas disponiveis: banco, cache, filas).
// Kubernetes usa o readiness probe para decidir se inclui o pod no load balancer.
// Se falhar, o pod para de receber requisicoes mas NAO e reiniciado.
public abstract class ReadinessHealthCheckBase : BedrockHealthCheckBase
{
    protected sealed override Task<HealthCheckOutput> CheckHealthInternalAsync(CancellationToken cancellationToken)
    {
        return CheckReadinessAsync(cancellationToken);
    }

    protected abstract Task<HealthCheckOutput> CheckReadinessAsync(CancellationToken cancellationToken);
}
