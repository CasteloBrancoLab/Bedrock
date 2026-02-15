using System.Reflection;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Microsoft.Extensions.Logging;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.TestMigrations;

/// <summary>
/// Concrete MigrationManagerBase for unit testing.
/// </summary>
internal sealed class TestMigrationManager : MigrationManagerBase
{
    private readonly string _connectionString;
    private readonly string _targetSchema;
    private readonly Assembly _migrationAssembly;

    protected override string ConnectionString => _connectionString;
    protected override string TargetSchema => _targetSchema;
    protected override Assembly MigrationAssembly => _migrationAssembly;

    public TestMigrationManager(
        ILogger logger,
        string connectionString,
        string targetSchema = "public",
        Assembly? migrationAssembly = null)
        : base(logger)
    {
        _connectionString = connectionString;
        _targetSchema = targetSchema;
        _migrationAssembly = migrationAssembly ?? typeof(TestMigrationManager).Assembly;
    }
}
