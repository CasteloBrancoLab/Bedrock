using System.Reflection;

namespace Bedrock.BuildingBlocks.Web.Hosting.Models;

// Informacoes imutaveis do host capturadas no momento do startup.
// Registrado como singleton no DI para uso em Swagger, health checks,
// logs estruturados, response headers, etc.
public sealed class StartupInfo
{
    public string AssemblyName { get; }
    public string Version { get; }
    public string Environment { get; }
    public DateTimeOffset StartedAt { get; }

    private StartupInfo(string assemblyName, string version, string environment, DateTimeOffset startedAt)
    {
        AssemblyName = assemblyName;
        Version = version;
        Environment = environment;
        StartedAt = startedAt;
    }

    internal static StartupInfo Create()
    {
        var entryAssembly = Assembly.GetEntryAssembly()!;
        var assemblyName = entryAssembly.GetName();

        var version = entryAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assemblyName.Version?.ToString()
            ?? "0.0.0";

        var environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Production";

        return new StartupInfo(
            assemblyName: assemblyName.Name ?? "Unknown",
            version: version,
            environment: environment,
            startedAt: DateTimeOffset.UtcNow);
    }
}
