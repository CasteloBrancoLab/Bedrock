using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;

namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Services;

// Base para health checks de liveness. Verifica se a aplicacao esta viva
// e funcionando (sem deadlocks, sem corrupcao de estado interno).
// Kubernetes usa o liveness probe para decidir se reinicia o pod.
// Deve ser leve e rapido — NAO verificar dependencias externas aqui.
public abstract class LivenessHealthCheckBase : BedrockHealthCheckBase
{
    protected sealed override Task<HealthCheckOutput> CheckHealthInternalAsync(CancellationToken cancellationToken)
    {
        return CheckLivenessAsync(cancellationToken);
    }

    protected abstract Task<HealthCheckOutput> CheckLivenessAsync(CancellationToken cancellationToken);
}
