using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Testing;

/// <summary>
/// Base fixture that provides IServiceCollection and IServiceProvider for dependency injection in tests.
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
/// }
/// </code>
/// </example>
public abstract class ServiceCollectionFixture : IDisposable
{
    private readonly Lazy<IServiceProvider> _serviceProvider;
    private bool _disposed;

    /// <summary>
    /// Gets the service collection for configuring dependencies.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// Built lazily after ConfigureServices is called.
    /// </summary>
    public IServiceProvider Provider => _serviceProvider.Value;

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
