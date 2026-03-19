using Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Models;

namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks.Services;

// Base para health checks de startup. Verifica se a aplicacao completou
// a inicializacao com sucesso (migrations, warm-up de caches, conexoes iniciais).
// Kubernetes usa o startup probe para saber quando comecar a enviar
// liveness/readiness probes — enquanto falhar, os outros probes sao suspensos.
public abstract class StartupHealthCheckBase : BedrockHealthCheckBase
{
    protected sealed override Task<HealthCheckOutput> CheckHealthInternalAsync(CancellationToken cancellationToken)
    {
        return CheckStartupAsync(cancellationToken);
    }

    protected abstract Task<HealthCheckOutput> CheckStartupAsync(CancellationToken cancellationToken);
}
