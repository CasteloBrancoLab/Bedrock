using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bedrock.BuildingBlocks.Testing.Integration.Environments;

/// <summary>
/// Information about an environment for reporting purposes.
/// </summary>
public sealed class EnvironmentReportInfo
{
    /// <summary>
    /// Gets or sets the environment name/key.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the list of services in the environment.
    /// </summary>
    [JsonPropertyName("services")]
    public required IReadOnlyList<ServiceReportInfo> Services { get; init; }

    /// <summary>
    /// Gets or sets the resource limits configuration.
    /// </summary>
    [JsonPropertyName("resources")]
    public ResourcesReportInfo? Resources { get; init; }

    /// <summary>
    /// Serializes this instance to JSON.
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, JsonContext.Default.EnvironmentReportInfo);
    }
}

/// <summary>
/// Information about a service in the environment.
/// </summary>
public sealed class ServiceReportInfo
{
    /// <summary>
    /// Gets or sets the service type (e.g., "PostgreSQL", "Redis", "RabbitMQ").
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Gets or sets the service key/name.
    /// </summary>
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    /// <summary>
    /// Gets or sets the Docker image used.
    /// </summary>
    [JsonPropertyName("image")]
    public required string Image { get; init; }

    /// <summary>
    /// Gets or sets the host address.
    /// </summary>
    [JsonPropertyName("host")]
    public string? Host { get; init; }

    /// <summary>
    /// Gets or sets the port number.
    /// </summary>
    [JsonPropertyName("port")]
    public int? Port { get; init; }

    /// <summary>
    /// Gets or sets the list of databases (for database services).
    /// </summary>
    [JsonPropertyName("databases")]
    public IReadOnlyList<string>? Databases { get; init; }

    /// <summary>
    /// Gets or sets the list of users (for database services).
    /// </summary>
    [JsonPropertyName("users")]
    public IReadOnlyList<string>? Users { get; init; }
}

/// <summary>
/// Information about resource limits.
/// </summary>
public sealed class ResourcesReportInfo
{
    /// <summary>
    /// Gets or sets the memory limit (e.g., "256m").
    /// </summary>
    [JsonPropertyName("memory")]
    public string? Memory { get; init; }

    /// <summary>
    /// Gets or sets the CPU limit (e.g., 0.5).
    /// </summary>
    [JsonPropertyName("cpu")]
    public double? Cpu { get; init; }
}

/// <summary>
/// JSON serialization context for report info classes.
/// </summary>
[JsonSerializable(typeof(EnvironmentReportInfo))]
[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class JsonContext : JsonSerializerContext;
