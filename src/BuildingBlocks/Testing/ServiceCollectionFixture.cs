using Bedrock.BuildingBlocks.Testing.Integration.Environments;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bedrock.BuildingBlocks.Testing;

/// <summary>
/// Base fixture that provides IServiceCollection, IServiceProvider, and
/// integration test environments for dependency injection in tests.
/// Implement ICollectionFixture&lt;T&gt; in derived classes to share across test classes.
/// </summary>
/// <example>
/// <code>
/// public class MyServiceFixture : ServiceCollectionFixture
/// {
///     protected override void ConfigureServices(IServiceCollection services)
///     {
///         services.AddSingleton&lt;IMyService, MyService&gt;();
///     }
///
///     protected override void ConfigureEnvironments(IEnvironmentRegistry environments)
///     {
///         environments.Register("test", env => env
///             .WithPostgres("main", pg => pg
///                 .WithDatabase("testdb")
///                 .WithUser("testuser", "testpass", user => user
///                     .WithSchemaPermission("public", PostgresSchemaPermission.Usage)
///                     .OnDatabase("testdb", db => db.OnAllTables(PostgresTablePermission.ReadWrite)))
///                 .WithResourceLimits(memory: "256m", cpu: 0.5)));
///     }
/// }
///
/// [CollectionDefinition("MyServices")]
/// public class MyServicesCollection : ICollectionFixture&lt;MyServiceFixture&gt; { }
///
/// [Collection("MyServices")]
/// public class MyTests : TestBase
/// {
///     private readonly MyServiceFixture _fixture;
///
///     public MyTests(MyServiceFixture fixture, ITestOutputHelper output) : base(output)
///     {
///         _fixture = fixture;
///     }
///
///     [Fact]
///     public async Task MyTest()
///     {
///         var connString = _fixture.Environments["test"]
///             .Postgres["main"]
///             .GetConnectionString("testdb", user: "testuser");
///     }
/// }
/// </code>
/// </example>
public abstract class ServiceCollectionFixture : IAsyncLifetime, IDisposable
{
    private readonly Lazy<IServiceProvider> _serviceProvider;
    private readonly EnvironmentRegistry _environments = new();
    private bool _disposed;
    private bool _environmentsInitialized;

    /// <summary>
    /// Gets the service collection for configuring dependencies.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// Built lazily after ConfigureServices is called.
    /// </summary>
    public IServiceProvider Provider => _serviceProvider.Value;

    /// <summary>
    /// Gets the environment registry for accessing integration test environments.
    /// </summary>
    public IEnvironmentRegistry Environments => _environments;

    protected ServiceCollectionFixture()
    {
        Services = new ServiceCollection();
        _serviceProvider = new Lazy<IServiceProvider>(() =>
        {
            ConfigureServices(Services);
            return Services.BuildServiceProvider();
        });
    }

    /// <summary>
    /// Override to configure services in the dependency injection container.
    /// </summary>
    protected abstract void ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Override to configure integration test environments.
    /// Default implementation does nothing (no environments).
    /// </summary>
    /// <param name="environments">The environment registry.</param>
    protected virtual void ConfigureEnvironments(IEnvironmentRegistry environments)
    {
        // Default: no environments configured
    }

    /// <summary>
    /// Resolves a service from the container.
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        return Provider.GetRequiredService<T>();
    }

    /// <summary>
    /// Tries to resolve a service from the container.
    /// </summary>
    public T? GetOptionalService<T>() where T : class
    {
        return Provider.GetService<T>();
    }

    /// <summary>
    /// Creates a new scope for scoped services.
    /// </summary>
    public IServiceScope CreateScope()
    {
        return Provider.CreateScope();
    }

    /// <summary>
    /// Initializes the fixture by configuring and starting all environments.
    /// </summary>
    public async Task InitializeAsync()
    {
        ConfigureEnvironments(_environments);

        if (_environments.All.Count > 0)
        {
            await _environments.InitializeAllAsync();
            _environmentsInitialized = true;
        }
    }

    /// <summary>
    /// Disposes all environments asynchronously.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_environmentsInitialized)
        {
            await _environments.DisposeAsync();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_serviceProvider.IsValueCreated && _serviceProvider.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
