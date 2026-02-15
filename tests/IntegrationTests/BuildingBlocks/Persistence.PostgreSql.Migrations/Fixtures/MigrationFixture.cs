using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Testing.Integration.Environments;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Migrations.TestMigrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using MessageType = Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums.MessageType;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Migrations.Fixtures;

public sealed class MigrationFixture : ServiceCollectionFixture
{
    private const string EnvironmentKey = "migration";
    private const string PostgresKey = "main";
    private const string DatabaseName = "testdb";

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug));
    }

    protected override void ConfigureEnvironments(IEnvironmentRegistry environments)
    {
        environments.Register(EnvironmentKey, env => env
            .WithPostgres(PostgresKey, pg => pg
                .WithImage("postgres:17")
                .WithDatabase(DatabaseName, db => { })
                .WithResourceLimits("256m", 0.5)));
    }

    public string GetConnectionString()
    {
        return Environments[EnvironmentKey]
            .Postgres[PostgresKey]
            .GetConnectionString(DatabaseName);
    }

    public TestMigrationManager CreateMigrationManager(string targetSchema = "public")
    {
        var logger = Provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<TestMigrationManager>();
        return new TestMigrationManager(logger, GetConnectionString(), targetSchema);
    }

    public static ExecutionContext CreateExecutionContext()
    {
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: Bedrock.BuildingBlocks.Core.TenantInfos.TenantInfo.Create(Guid.Empty),
            executionUser: "integration-test",
            executionOrigin: "test-runner",
            businessOperationCode: "TEST_MIGRATION",
            minimumMessageType: MessageType.Information,
            timeProvider: TimeProvider.System);
    }
}

[CollectionDefinition("MigrationTests")]
public sealed class MigrationCollection : ICollectionFixture<MigrationFixture>;
