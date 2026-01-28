namespace Bedrock.BuildingBlocks.Testing.Integration.Environments;

/// <summary>
/// Registry for integration test environments.
/// </summary>
public interface IEnvironmentRegistry
{
    /// <summary>
    /// Registers an environment with the given key.
    /// </summary>
    /// <param name="key">The environment key.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>This registry for method chaining.</returns>
    IEnvironmentRegistry Register(
        string key,
        Action<IntegrationTestEnvironment> configure);

    /// <summary>
    /// Gets an environment by key.
    /// </summary>
    /// <param name="key">The environment key.</param>
    /// <returns>The environment.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the environment is not found.</exception>
    IIntegrationTestEnvironment this[string key] { get; }

    /// <summary>
    /// Gets all registered environments.
    /// </summary>
    IReadOnlyDictionary<string, IIntegrationTestEnvironment> All { get; }
}
