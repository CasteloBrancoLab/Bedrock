using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Builders;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Runtime;
using Docker.DotNet.Models;
using Testcontainers.PostgreSql;

namespace Bedrock.BuildingBlocks.Testing.Integration.Environments;

/// <summary>
/// Implementation of an integration test environment.
/// </summary>
public sealed class IntegrationTestEnvironment : IIntegrationTestEnvironment
{
    private readonly string _key;
    private readonly List<PostgresContainerConfig> _postgresConfigs = [];
    private readonly Dictionary<string, PostgresContainerWrapper> _postgresContainers = [];

    internal IntegrationTestEnvironment(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _key = key;
    }

    /// <inheritdoc />
    public string Key => _key;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, PostgresContainerWrapper> Postgres => _postgresContainers;

    /// <summary>
    /// Adds a PostgreSQL container to this environment.
    /// </summary>
    /// <param name="key">The container key.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>This environment for method chaining.</returns>
    public IntegrationTestEnvironment WithPostgres(
        string key,
        Action<PostgresContainerBuilder> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new PostgresContainerBuilder(key);
        configure(builder);
        _postgresConfigs.Add(builder.Build());

        return this;
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        foreach (var config in _postgresConfigs)
        {
            var container = await CreateAndStartPostgresAsync(config, cancellationToken);
            _postgresContainers[config.Key] = container;
        }
    }

    private static async Task<PostgresContainerWrapper> CreateAndStartPostgresAsync(
        PostgresContainerConfig config,
        CancellationToken cancellationToken)
    {
        var builder = new PostgreSqlBuilder()
            .WithImage(config.Image)
            .WithDatabase("postgres")
            .WithUsername("postgres")
            .WithPassword("postgres");

        // Apply resource limits if specified using the generic CreateParameterModifier
        if (config.MemoryLimit is not null || config.CpuLimit is not null)
        {
            builder = builder.WithCreateParameterModifier(parameters =>
            {
                parameters.HostConfig ??= new HostConfig();

                if (config.MemoryLimit is not null)
                {
                    parameters.HostConfig.Memory = ParseMemoryLimit(config.MemoryLimit);
                }

                if (config.CpuLimit is not null)
                {
                    // NanoCPUs: 1 CPU = 1e9 nanocpus
                    parameters.HostConfig.NanoCPUs = (long)(config.CpuLimit.Value * 1_000_000_000);
                }
            });
        }

        var testcontainer = builder.Build();
        await testcontainer.StartAsync(cancellationToken);

        var wrapper = new PostgresContainerWrapper(testcontainer, config);
        await wrapper.InitializeAsync(cancellationToken);

        return wrapper;
    }

    private static long ParseMemoryLimit(string limit)
    {
        var normalized = limit.ToLowerInvariant().Trim();

        if (normalized.EndsWith("gb"))
        {
            return long.Parse(normalized[..^2]) * 1024 * 1024 * 1024;
        }

        if (normalized.EndsWith("g"))
        {
            return long.Parse(normalized[..^1]) * 1024 * 1024 * 1024;
        }

        if (normalized.EndsWith("mb"))
        {
            return long.Parse(normalized[..^2]) * 1024 * 1024;
        }

        if (normalized.EndsWith("m"))
        {
            return long.Parse(normalized[..^1]) * 1024 * 1024;
        }

        if (normalized.EndsWith("kb"))
        {
            return long.Parse(normalized[..^2]) * 1024;
        }

        if (normalized.EndsWith("k"))
        {
            return long.Parse(normalized[..^1]) * 1024;
        }

        return long.Parse(normalized);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var container in _postgresContainers.Values)
        {
            await container.DisposeAsync();
        }

        _postgresContainers.Clear();
    }
}
