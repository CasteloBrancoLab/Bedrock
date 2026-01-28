using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Integration.Environments;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Fixtures;

/// <summary>
/// Fixture for PostgreSQL repository integration tests.
/// Provides a configured PostgreSQL container with test databases and users.
/// </summary>
public class PostgresRepositoryFixture : ServiceCollectionFixture
{
    /// <summary>
    /// Gets the connection string for the test database with admin access.
    /// </summary>
    public string GetAdminConnectionString()
    {
        return Environments["repository"]
            .Postgres["main"]
            .GetConnectionString("testdb");
    }

    /// <summary>
    /// Gets the connection string for the test database with app user access.
    /// </summary>
    public string GetAppUserConnectionString()
    {
        return Environments["repository"]
            .Postgres["main"]
            .GetConnectionString("testdb", user: "app_user");
    }

    /// <summary>
    /// Gets the connection string for the test database with readonly user access.
    /// </summary>
    public string GetReadonlyUserConnectionString()
    {
        return Environments["repository"]
            .Postgres["main"]
            .GetConnectionString("testdb", user: "readonly_user");
    }

    /// <inheritdoc />
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Add any services needed for tests
    }

    /// <inheritdoc />
    protected override void ConfigureEnvironments(IEnvironmentRegistry environments)
    {
        environments.Register("repository", env => env
            .WithPostgres("main", pg => pg
                .WithImage("postgres:17")
                .WithDatabase("testdb", db => db
                    .WithSeedSql("""
                        CREATE TABLE IF NOT EXISTS test_entities (
                            id UUID PRIMARY KEY,
                            tenant_code UUID NOT NULL,
                            name TEXT NOT NULL,
                            created_by TEXT NOT NULL,
                            created_at TIMESTAMPTZ NOT NULL,
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
                    .OnDatabase("testdb", db => db
                        .OnAllTables(PostgresTablePermission.ReadWrite)
                        .OnAllSequences(PostgresSequencePermission.All)))
                .WithUser("readonly_user", "readonly_password", user => user
                    .WithSchemaPermission("public", PostgresSchemaPermission.Usage)
                    .OnDatabase("testdb", db => db
                        .OnAllTables(PostgresTablePermission.ReadOnly)))
                .WithResourceLimits(memory: "256m", cpu: 0.5)));
    }
}
