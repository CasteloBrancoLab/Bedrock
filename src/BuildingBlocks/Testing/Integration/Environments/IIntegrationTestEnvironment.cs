using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Runtime;

namespace Bedrock.BuildingBlocks.Testing.Integration.Environments;

/// <summary>
/// Represents a configured integration test environment.
/// </summary>
public interface IIntegrationTestEnvironment : IAsyncDisposable
{
    /// <summary>
    /// Gets the environment key.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets PostgreSQL containers by key.
    /// </summary>
    IReadOnlyDictionary<string, PostgresContainerWrapper> Postgres { get; }

    /// <summary>
    /// Initializes all containers in the environment.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
