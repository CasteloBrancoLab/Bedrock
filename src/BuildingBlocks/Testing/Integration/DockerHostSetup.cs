using System.Runtime.CompilerServices;

namespace Bedrock.BuildingBlocks.Testing.Integration;

/// <summary>
/// Auto-configures Testcontainers for remote Docker (TCP) environments such as WSL2.
/// <para>
/// Fixes two common issues when Docker runs inside WSL2 with TCP exposure:
/// <list type="bullet">
///   <item>
///     <description>
///       <b>IPv6 timeout:</b> Replaces <c>localhost</c> with <c>127.0.0.1</c> in DOCKER_HOST
///       to avoid 21s+ timeouts caused by .NET trying IPv6 (::1) first when Docker only listens on IPv4.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>Ryuk socket mount:</b> Sets TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE to /var/run/docker.sock
///       so the resource reaper container can connect to the Docker daemon.
///     </description>
///   </item>
/// </list>
/// </para>
/// </summary>
internal static class DockerHostSetup
{
    private const string DockerSocketOverrideVar = "TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE";
    private const string DockerHostVar = "DOCKER_HOST";
    private const string DefaultDockerSocket = "/var/run/docker.sock";

    [ModuleInitializer]
    internal static void Configure()
    {
        var dockerHost = Environment.GetEnvironmentVariable(DockerHostVar);

        if (dockerHost is null || !dockerHost.StartsWith("tcp://", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Replace "localhost" with "127.0.0.1" to force IPv4 and avoid IPv6 timeout
        if (dockerHost.Contains("://localhost", StringComparison.OrdinalIgnoreCase))
        {
            dockerHost = dockerHost.Replace("://localhost", "://127.0.0.1", StringComparison.OrdinalIgnoreCase);
            Environment.SetEnvironmentVariable(DockerHostVar, dockerHost);
        }

        // Set Docker socket override for Ryuk resource reaper
        var socketOverride = Environment.GetEnvironmentVariable(DockerSocketOverrideVar);

        if (string.IsNullOrEmpty(socketOverride))
        {
            Environment.SetEnvironmentVariable(DockerSocketOverrideVar, DefaultDockerSocket);
        }
    }
}
