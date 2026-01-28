namespace Bedrock.BuildingBlocks.Testing.Integration.Environments;

/// <summary>
/// Implementation of the environment registry.
/// Thread-safe for concurrent access.
/// </summary>
public sealed class EnvironmentRegistry : IEnvironmentRegistry, IAsyncDisposable
{
    private readonly Dictionary<string, IntegrationTestEnvironment> _environments = [];
    private readonly object _lock = new();

    /// <inheritdoc />
    public IEnvironmentRegistry Register(
        string key,
        Action<IntegrationTestEnvironment> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(configure);

        lock (_lock)
        {
            if (_environments.ContainsKey(key))
            {
                throw new InvalidOperationException($"Environment '{key}' is already registered");
            }

            var environment = new IntegrationTestEnvironment(key);
            configure(environment);
            _environments[key] = environment;
        }

        return this;
    }

    /// <inheritdoc />
    public IIntegrationTestEnvironment this[string key]
    {
        get
        {
            lock (_lock)
            {
                if (!_environments.TryGetValue(key, out var env))
                {
                    throw new KeyNotFoundException(
                        $"Environment '{key}' not found. Available: {string.Join(", ", _environments.Keys)}");
                }

                return env;
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IIntegrationTestEnvironment> All
    {
        get
        {
            lock (_lock)
            {
                return _environments
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => (IIntegrationTestEnvironment)kvp.Value)
                    .AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Initializes all registered environments.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    internal async Task InitializeAllAsync(CancellationToken cancellationToken = default)
    {
        List<IntegrationTestEnvironment> envs;
        lock (_lock)
        {
            envs = [.. _environments.Values];
        }

        // Initialize environments in parallel for faster startup
        await Parallel.ForEachAsync(
            envs,
            cancellationToken,
            async (env, ct) => await env.InitializeAsync(ct));
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        List<IntegrationTestEnvironment> envs;
        lock (_lock)
        {
            envs = [.. _environments.Values];
            _environments.Clear();
        }

        foreach (var env in envs)
        {
            await env.DisposeAsync();
        }
    }
}
