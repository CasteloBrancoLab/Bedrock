using Bedrock.BuildingBlocks.Testing.Integration.Environments;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Runtime;

namespace Bedrock.PerformanceTests.BuildingBlocks.Persistence.PostgreSql.Infrastructure;

/// <summary>
/// Static container lifecycle manager for benchmarks.
/// Provides a PostgreSQL container with the same configuration as
/// <c>PostgresRepositoryFixture</c> from integration tests.
/// <para>
/// Since BenchmarkDotNet does not use xUnit fixtures, this class manages
/// the container lifecycle statically with thread-safe initialization.
/// The container is started once and shared across all benchmarks.
/// </para>
/// </summary>
public static class PostgresBenchmarkSetup
{
    private static EnvironmentRegistry? _registry;
    private static IIntegrationTestEnvironment? _environment;
    private static readonly SemaphoreSlim Lock = new(1, 1);

    /// <summary>
    /// Ensures the PostgreSQL container is started and returns the connection string
    /// for the test database with app user credentials.
    /// </summary>
    /// <returns>Connection string for the benchmark database.</returns>
    public static async Task<string> EnsureStartedAsync()
    {
        if (_environment is not null)
            return GetAppUserConnectionString();

        await Lock.WaitAsync();
        try
        {
            if (_environment is not null)
                return GetAppUserConnectionString();

            var registry = new EnvironmentRegistry();
            registry.Register("benchmark", env => env
                .WithPostgres("main", pg => pg
                    .WithImage("postgres:17")
                    .WithDatabase("benchdb", db => db
                        .WithSeedSql("""
                            CREATE TABLE IF NOT EXISTS test_entities (
                                id UUID PRIMARY KEY,
                                tenant_code UUID NOT NULL,
                                name TEXT NOT NULL,
                                created_by TEXT NOT NULL,
                                created_at TIMESTAMPTZ NOT NULL,
                                created_correlation_id UUID NOT NULL,
                                created_execution_origin TEXT NOT NULL,
                                created_business_operation_code TEXT NOT NULL,
                                last_changed_by TEXT,
                                last_changed_at TIMESTAMPTZ,
                                last_changed_execution_origin TEXT,
                                last_changed_correlation_id UUID,
                                last_changed_business_operation_code TEXT,
                                entity_version BIGINT NOT NULL DEFAULT 1
                            );

                            CREATE INDEX IF NOT EXISTS idx_test_entities_tenant_id
                                ON test_entities(tenant_code, id);
                            """))
                    .WithUser("app_user", "app_password", user => user
                        .WithSchemaPermission("public", PostgresSchemaPermission.Usage)
                        .OnDatabase("benchdb", db => db
                            .OnAllTables(PostgresTablePermission.ReadWrite)
                            .OnAllSequences(PostgresSequencePermission.All)))
                    .WithResourceLimits(memory: "256m", cpu: 0.5)));

            // InitializeAllAsync is internal, so we access the environment and initialize it
            // through the registry's public API
            var env = registry["benchmark"];
            await env.InitializeAsync();

            _registry = registry;
            _environment = env;
        }
        finally
        {
            Lock.Release();
        }

        return GetAppUserConnectionString();
    }

    /// <summary>
    /// Gets the connection string for the admin (postgres) user.
    /// </summary>
    public static string GetAdminConnectionString()
    {
        EnsureInitialized();
        return _environment!.Postgres["main"].GetConnectionString("benchdb");
    }

    /// <summary>
    /// Gets the connection string for the app user.
    /// </summary>
    public static string GetAppUserConnectionString()
    {
        EnsureInitialized();
        return _environment!.Postgres["main"].GetConnectionString("benchdb", user: "app_user");
    }

    /// <summary>
    /// Gets the PostgreSQL container wrapper for direct access.
    /// </summary>
    public static PostgresContainerWrapper GetContainer()
    {
        EnsureInitialized();
        return _environment!.Postgres["main"];
    }

    /// <summary>
    /// Stops and disposes the PostgreSQL container.
    /// Should be called from [GlobalCleanup] of the last benchmark.
    /// </summary>
    public static async Task StopAsync()
    {
        if (_registry is not null)
        {
            await _registry.DisposeAsync();
            _registry = null;
            _environment = null;
        }
    }

    private static void EnsureInitialized()
    {
        if (_environment is null)
            throw new InvalidOperationException(
                "PostgresBenchmarkSetup has not been initialized. Call EnsureStartedAsync() first.");
    }
}
