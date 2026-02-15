using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;

/// <summary>
/// Abstract base class for migration management.
/// Each bounded context creates a concrete implementation providing
/// its own connection string, schema, and migration assembly.
/// </summary>
public abstract class MigrationManagerBase
{
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the PostgreSQL connection string for the target database.
    /// </summary>
    protected abstract string ConnectionString { get; }

    /// <summary>
    /// Gets the schema where migrations are applied (e.g., "public").
    /// </summary>
    protected abstract string TargetSchema { get; }

    /// <summary>
    /// Gets the assembly containing migration classes and embedded SQL scripts.
    /// </summary>
    protected abstract Assembly MigrationAssembly { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="MigrationManagerBase"/>.
    /// </summary>
    /// <param name="logger">The logger for distributed tracing.</param>
    protected MigrationManagerBase(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Applies all pending migrations in ascending version order.
    /// </summary>
    /// <param name="executionContext">The execution context for distributed tracing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Delegacao para metodo interno excluido - coberto por testes de integracao")]
    public async Task MigrateUpAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(executionContext);
        await MigrateUpInternalAsync(executionContext, cancellationToken);
    }
    // Stryker restore all

    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    private async Task MigrateUpInternalAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        _logger.LogInformationForDistributedTracing(
            executionContext,
            "Starting migration up for schema {Schema}",
            TargetSchema);

        try
        {
            await Task.Run(ExecuteMigrateUp, cancellationToken);

            _logger.LogInformationForDistributedTracing(
                executionContext,
                "Migration up completed successfully for schema {Schema}",
                TargetSchema);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "Migration up failed for schema {Schema}",
                TargetSchema);
            throw;
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    private void ExecuteMigrateUp()
    {
        using var serviceProvider = CreateServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    /// <summary>
    /// Rollback migrations to the specified target version (exclusive).
    /// Executes DOWN scripts in descending version order.
    /// </summary>
    /// <param name="executionContext">The execution context for distributed tracing.</param>
    /// <param name="targetVersion">The version to rollback to (exclusive — migrations above this version are reverted).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown when a DOWN script is missing for a migration that needs to be reverted.</exception>
    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Delegacao para metodo interno excluido - coberto por testes de integracao")]
    public async Task MigrateDownAsync(
        ExecutionContext executionContext,
        long targetVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(executionContext);
        await MigrateDownInternalAsync(executionContext, targetVersion, cancellationToken);
    }
    // Stryker restore all

    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    private async Task MigrateDownInternalAsync(
        ExecutionContext executionContext,
        long targetVersion,
        CancellationToken cancellationToken)
    {
        _logger.LogInformationForDistributedTracing(
            executionContext,
            "Starting migration down to version {TargetVersion} for schema {Schema}",
            targetVersion,
            TargetSchema);

        try
        {
            await Task.Run(() => ExecuteMigrateDown(targetVersion), cancellationToken);

            _logger.LogInformationForDistributedTracing(
                executionContext,
                "Migration down to version {TargetVersion} completed successfully for schema {Schema}",
                targetVersion,
                TargetSchema);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "Migration down to version {TargetVersion} failed for schema {Schema}",
                targetVersion,
                TargetSchema);
            throw;
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    private void ExecuteMigrateDown(long targetVersion)
    {
        using var serviceProvider = CreateServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateDown(targetVersion);
    }

    /// <summary>
    /// Query the current migration status without making changes.
    /// Returns applied and pending migrations.
    /// </summary>
    /// <param name="executionContext">The execution context for distributed tracing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current migration status with applied and pending migration lists.</returns>
    // Stryker disable all : Requer conexao PostgreSQL real - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Delegacao para metodo interno excluido - coberto por testes de integracao")]
    public async Task<Models.MigrationStatus> GetStatusAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(executionContext);
        return await GetStatusInternalAsync(executionContext, cancellationToken);
    }
    // Stryker restore all

    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    private async Task<Models.MigrationStatus> GetStatusInternalAsync(
        ExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        _logger.LogInformationForDistributedTracing(
            executionContext,
            "Querying migration status for schema {Schema}",
            TargetSchema);

        try
        {
            var status = await Task.Run(ExecuteGetStatus, cancellationToken);

            _logger.LogInformationForDistributedTracing(
                executionContext,
                "Migration status for schema {Schema}: {Applied} applied, {Pending} pending",
                TargetSchema,
                status.AppliedMigrations.Count,
                status.PendingMigrations.Count);

            return status;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "Failed to query migration status for schema {Schema}",
                TargetSchema);
            throw;
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Requer conexao PostgreSQL real - coberto por testes de integracao")]
    private Models.MigrationStatus ExecuteGetStatus()
    {
        using var serviceProvider = CreateServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.LoadVersionInfoIfRequired();

        var loader = runner.MigrationLoader;
        var versionLoader = scope.ServiceProvider
            .GetRequiredService<IVersionLoader>();

        var versionData = versionLoader.VersionInfo;
        var allMigrations = loader.LoadMigrations();

        var applied = new List<Models.MigrationInfo>();
        var pending = new List<Models.MigrationInfo>();

        foreach (var migration in allMigrations.OrderBy(m => m.Key))
        {
            var version = migration.Key;
            var description = migration.Value.Migration.GetType().Name;

            if (versionData.HasAppliedMigration(version))
            {
                var appliedOn = versionData.AppliedMigrations()
                    .FirstOrDefault(v => v == version);
                applied.Add(Models.MigrationInfo.Create(
                    version,
                    description,
                    appliedOn > 0 ? DateTimeOffset.UtcNow : null));
            }
            else
            {
                pending.Add(Models.MigrationInfo.Create(version, description));
            }
        }

        return Models.MigrationStatus.Create(applied, pending);
    }

    /// <summary>
    /// Creates a FluentMigrator service provider configured for this manager.
    /// </summary>
    // Stryker disable once Boolean : validateScopes=false is a configuration choice for FluentMigrator
    internal ServiceProvider CreateServiceProvider()
    {
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(ConnectionString)
                .ScanIn(MigrationAssembly).For.All())
            .AddScoped<IConventionSet>(_ => new DefaultConventionSet(TargetSchema, null))
            .AddLogging()
            .BuildServiceProvider(false);
    }
}
