using Bedrock.BuildingBlocks.Testing.Integration;
using Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Migrations.Fixtures;
using Npgsql;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Migrations;

[Collection("MigrationTests")]
public class MigrationManagerIntegrationTests : IntegrationTestBase
{
    private readonly MigrationFixture _fixture;

    public MigrationManagerIntegrationTests(MigrationFixture fixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _fixture = fixture;
        UseEnvironment(_fixture.Environments["migration"]);
    }

    [Fact]
    public async Task MigrateUpAsync_SingleMigration_ShouldApplySchemaChanges()
    {
        // Arrange
        LogArrange("Creating migration manager and execution context");
        var manager = _fixture.CreateMigrationManager();
        var executionContext = MigrationFixture.CreateExecutionContext();

        // Act
        LogAct("Executing MigrateUpAsync");
        await manager.MigrateUpAsync(executionContext);

        // Assert
        LogAssert("Verifying schema changes were applied");
        var tableExists = await TableExistsAsync("test_table");
        tableExists.ShouldBeTrue("test_table should exist after migration");
    }

    [Fact]
    public async Task MigrateUpAsync_MultipleMigrations_ShouldApplyInOrder()
    {
        // Arrange
        LogArrange("Creating migration manager and execution context");
        var manager = _fixture.CreateMigrationManager();
        var executionContext = MigrationFixture.CreateExecutionContext();

        // Act
        LogAct("Executing MigrateUpAsync for multiple migrations");
        await manager.MigrateUpAsync(executionContext);

        // Assert
        LogAssert("Verifying all migrations were applied in order");
        var tableExists = await TableExistsAsync("test_table");
        tableExists.ShouldBeTrue("test_table should exist");

        var columnExists = await ColumnExistsAsync("test_table", "description");
        columnExists.ShouldBeTrue("description column should exist after second migration");
    }

    [Fact]
    public async Task MigrateUpAsync_NoPendingMigrations_ShouldBeNoOp()
    {
        // Arrange
        LogArrange("Applying all migrations first");
        var manager = _fixture.CreateMigrationManager();
        var executionContext = MigrationFixture.CreateExecutionContext();
        await manager.MigrateUpAsync(executionContext);

        // Act
        LogAct("Executing MigrateUpAsync again (no pending migrations)");
        var secondContext = MigrationFixture.CreateExecutionContext();
        await manager.MigrateUpAsync(secondContext);

        // Assert
        LogAssert("Verifying no error occurred on second run");
        var tableExists = await TableExistsAsync("test_table");
        tableExists.ShouldBeTrue("test_table should still exist");
    }

    [Fact]
    public async Task MigrateDownAsync_RollbackSingleMigration_ShouldRevertSchemaChanges()
    {
        // Arrange
        LogArrange("Applying all migrations first");
        var manager = _fixture.CreateMigrationManager();
        var executionContext = MigrationFixture.CreateExecutionContext();
        await manager.MigrateUpAsync(executionContext);

        // Act
        LogAct("Rolling back to version before second migration");
        var rollbackContext = MigrationFixture.CreateExecutionContext();
        await manager.MigrateDownAsync(rollbackContext, 202602140001);

        // Assert
        LogAssert("Verifying second migration was reverted");
        var tableExists = await TableExistsAsync("test_table");
        tableExists.ShouldBeTrue("test_table should still exist (first migration not reverted)");

        var columnExists = await ColumnExistsAsync("test_table", "description");
        columnExists.ShouldBeFalse("description column should not exist after rollback");
    }

    [Fact]
    public async Task MigrateDownAsync_RollbackAllMigrations_ShouldRevertAllSchemaChanges()
    {
        // Arrange
        LogArrange("Applying all migrations first");
        var manager = _fixture.CreateMigrationManager();
        var executionContext = MigrationFixture.CreateExecutionContext();
        await manager.MigrateUpAsync(executionContext);

        // Act
        LogAct("Rolling back all migrations (target version 0)");
        var rollbackContext = MigrationFixture.CreateExecutionContext();
        await manager.MigrateDownAsync(rollbackContext, 0);

        // Assert
        LogAssert("Verifying all migrations were reverted");
        var tableExists = await TableExistsAsync("test_table");
        tableExists.ShouldBeFalse("test_table should not exist after full rollback");
    }

    [Fact]
    public async Task MigrateDownAsync_OnEmptyDatabase_ShouldBeNoOp()
    {
        // Arrange
        LogArrange("Creating migration manager on fresh database");
        var manager = _fixture.CreateMigrationManager();
        var executionContext = MigrationFixture.CreateExecutionContext();

        // Act
        LogAct("Rolling back on empty database (no-op)");
        await manager.MigrateDownAsync(executionContext, 0);

        // Assert
        LogAssert("Verifying no error occurred");
        var tableExists = await TableExistsAsync("test_table");
        tableExists.ShouldBeFalse("test_table should not exist on empty database");
    }

    [Fact]
    public async Task GetStatusAsync_MixedAppliedAndPending_ShouldReturnCorrectStatus()
    {
        // Arrange
        LogArrange("Applying first migration only by migrating up then down");
        var manager = _fixture.CreateMigrationManager();
        var executionContext = MigrationFixture.CreateExecutionContext();
        await manager.MigrateUpAsync(executionContext);
        await manager.MigrateDownAsync(MigrationFixture.CreateExecutionContext(), 202602140001);

        // Act
        LogAct("Querying migration status");
        var statusContext = MigrationFixture.CreateExecutionContext();
        var status = await manager.GetStatusAsync(statusContext);

        // Assert
        LogAssert("Verifying applied and pending migration counts");
        status.ShouldNotBeNull();
        status.AppliedMigrations.Count.ShouldBe(1);
        status.PendingMigrations.Count.ShouldBe(1);
        status.HasPendingMigrations.ShouldBeTrue();
        status.LastAppliedVersion.ShouldBe(202602140001);
    }

    [Fact]
    public async Task GetStatusAsync_NewDatabase_ShouldReportAllPending()
    {
        // Arrange
        LogArrange("Creating migration manager on a fresh schema with no version history");
        var freshSchema = "fresh_status_test";
        await CreateSchemaAsync(freshSchema);
        var manager = _fixture.CreateMigrationManager(freshSchema);
        var executionContext = MigrationFixture.CreateExecutionContext();

        // Act
        LogAct("Querying migration status on new schema");
        var status = await manager.GetStatusAsync(executionContext);

        // Assert
        LogAssert("Verifying all migrations are pending");
        status.ShouldNotBeNull();
        status.AppliedMigrations.ShouldBeEmpty();
        status.PendingMigrations.Count.ShouldBeGreaterThan(0);
        status.HasPendingMigrations.ShouldBeTrue();
        status.LastAppliedVersion.ShouldBeNull();
    }

    [Fact]
    public async Task MigrateUpAsync_WithCustomSchema_ShouldTrackVersionInfoInConfiguredSchema()
    {
        // Arrange
        LogArrange("Creating schema and migration manager with custom schema");
        var customSchema = "custom_bc";
        await CreateSchemaAsync(customSchema);
        var manager = _fixture.CreateMigrationManager(customSchema);
        var executionContext = MigrationFixture.CreateExecutionContext();

        // Act
        LogAct("Executing MigrateUpAsync with custom schema");
        await manager.MigrateUpAsync(executionContext);

        // Assert
        LogAssert("Verifying VersionInfo table was created in custom schema");
        var versionTableExists = await TableExistsInSchemaAsync("VersionInfo", customSchema);
        versionTableExists.ShouldBeTrue("VersionInfo should be tracked in custom schema");

        var status = await manager.GetStatusAsync(MigrationFixture.CreateExecutionContext());
        status.AppliedMigrations.Count.ShouldBe(2, "All migrations should be applied");
        status.PendingMigrations.ShouldBeEmpty();
    }

    [Fact]
    public async Task MigrateUpAsync_WithSpecificAssembly_ShouldScanOnlyThatAssembly()
    {
        // Arrange
        LogArrange("Creating migration manager with specific assembly");
        var manager = _fixture.CreateMigrationManager();
        var executionContext = MigrationFixture.CreateExecutionContext();

        // Act
        LogAct("Executing MigrateUpAsync and querying status");
        await manager.MigrateUpAsync(executionContext);
        var status = await manager.GetStatusAsync(MigrationFixture.CreateExecutionContext());

        // Assert
        LogAssert("Verifying only migrations from configured assembly were applied");
        status.AppliedMigrations.Count.ShouldBe(2, "Should have exactly 2 migrations from test assembly");
        status.PendingMigrations.ShouldBeEmpty();
    }

    private async Task CreateSchemaAsync(string schemaName)
    {
        await using var connection = new NpgsqlConnection(_fixture.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA IF NOT EXISTS {schemaName}";
        await command.ExecuteNonQueryAsync();
    }

    private async Task<bool> TableExistsInSchemaAsync(string tableName, string schemaName)
    {
        await using var connection = new NpgsqlConnection(_fixture.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT EXISTS (
                SELECT FROM information_schema.tables
                WHERE table_schema = '{schemaName}' AND table_name = '{tableName}'
            )
            """;
        var result = await command.ExecuteScalarAsync();
        return result is true;
    }

    private async Task<bool> TableExistsAsync(string tableName)
    {
        await using var connection = new NpgsqlConnection(_fixture.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT EXISTS (
                SELECT FROM information_schema.tables
                WHERE table_schema = 'public' AND table_name = '{tableName}'
            )
            """;
        var result = await command.ExecuteScalarAsync();
        return result is true;
    }

    private async Task<bool> ColumnExistsAsync(string tableName, string columnName)
    {
        await using var connection = new NpgsqlConnection(_fixture.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT EXISTS (
                SELECT FROM information_schema.columns
                WHERE table_schema = 'public' AND table_name = '{tableName}' AND column_name = '{columnName}'
            )
            """;
        var result = await command.ExecuteScalarAsync();
        return result is true;
    }
}
