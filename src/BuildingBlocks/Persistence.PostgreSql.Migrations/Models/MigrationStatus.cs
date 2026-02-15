namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Models;

/// <summary>
/// Aggregated status of all migrations for a bounded context.
/// </summary>
public sealed class MigrationStatus
{
    /// <summary>
    /// Gets the list of migrations already applied, ordered by version ascending.
    /// </summary>
    public IReadOnlyList<MigrationInfo> AppliedMigrations { get; }

    /// <summary>
    /// Gets the list of migrations not yet applied, ordered by version ascending.
    /// </summary>
    public IReadOnlyList<MigrationInfo> PendingMigrations { get; }

    /// <summary>
    /// Gets the version of the most recently applied migration, or null if none.
    /// </summary>
    public long? LastAppliedVersion { get; }

    /// <summary>
    /// Gets a value indicating whether there are unapplied migrations.
    /// </summary>
    public bool HasPendingMigrations { get; }

    private MigrationStatus(
        IReadOnlyList<MigrationInfo> appliedMigrations,
        IReadOnlyList<MigrationInfo> pendingMigrations)
    {
        AppliedMigrations = appliedMigrations;
        PendingMigrations = pendingMigrations;
        LastAppliedVersion = appliedMigrations.Count > 0 ? appliedMigrations[^1].Version : null;
        HasPendingMigrations = pendingMigrations.Count > 0;
    }

    /// <summary>
    /// Creates a new <see cref="MigrationStatus"/> instance.
    /// </summary>
    /// <param name="appliedMigrations">Applied migrations ordered by version ascending.</param>
    /// <param name="pendingMigrations">Pending migrations ordered by version ascending.</param>
    public static MigrationStatus Create(
        IReadOnlyList<MigrationInfo> appliedMigrations,
        IReadOnlyList<MigrationInfo> pendingMigrations)
    {
        ArgumentNullException.ThrowIfNull(appliedMigrations);
        ArgumentNullException.ThrowIfNull(pendingMigrations);

        return new MigrationStatus(appliedMigrations, pendingMigrations);
    }
}
