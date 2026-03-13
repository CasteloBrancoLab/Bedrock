namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks;

// Armazena a configuracao dos 3 probes de health check (startup, readiness, liveness)
// com seus respectivos tipos e paths. Registrado como singleton no DI para que
// o MapBedrockHealthChecks consiga mapear os endpoints automaticamente.
public sealed class BedrockHealthCheckOptions
{
    internal Type? StartupCheckType { get; private set; }
    internal string? StartupPath { get; private set; }

    internal Type? ReadinessCheckType { get; private set; }
    internal string? ReadinessPath { get; private set; }

    internal Type? LivenessCheckType { get; private set; }
    internal string? LivenessPath { get; private set; }

    internal const string StartupTag = "startup";
    internal const string ReadinessTag = "readiness";
    internal const string LivenessTag = "liveness";

    public BedrockHealthCheckOptions AddStartupCheck<TCheck>(string path)
        where TCheck : StartupHealthCheckBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        StartupCheckType = typeof(TCheck);
        StartupPath = path;
        return this;
    }

    public BedrockHealthCheckOptions AddReadinessCheck<TCheck>(string path)
        where TCheck : ReadinessHealthCheckBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        ReadinessCheckType = typeof(TCheck);
        ReadinessPath = path;
        return this;
    }

    public BedrockHealthCheckOptions AddLivenessCheck<TCheck>(string path)
        where TCheck : LivenessHealthCheckBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        LivenessCheckType = typeof(TCheck);
        LivenessPath = path;
        return this;
    }
}
