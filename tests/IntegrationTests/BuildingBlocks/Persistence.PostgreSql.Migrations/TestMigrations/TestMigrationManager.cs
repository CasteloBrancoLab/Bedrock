using System.Reflection;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Microsoft.Extensions.Logging;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Migrations.TestMigrations;

/// <summary>
/// Concrete MigrationManagerBase for integration testing.
/// </summary>
public sealed class TestMigrationManager : MigrationManagerBase
{
    private readonly string _connectionString;
    private readonly string _targetSchema;

    protected override string ConnectionString => _connectionString;
    protected override string TargetSchema => _targetSchema;
    protected override Assembly MigrationAssembly => typeof(TestMigrationManager).Assembly;

    public TestMigrationManager(ILogger logger, string connectionString, string targetSchema = "public")
        : base(logger)
    {
        _connectionString = connectionString;
        _targetSchema = targetSchema;
    }
}
