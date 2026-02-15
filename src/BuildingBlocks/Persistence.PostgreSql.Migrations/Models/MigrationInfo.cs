namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Models;

/// <summary>
/// Represents a single migration record (applied or pending).
/// </summary>
public readonly record struct MigrationInfo
{
    /// <summary>
    /// Gets the migration version number (timestamp format YYYYMMDDHHmm).
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// Gets the human-readable description derived from the script filename.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the timestamp when the migration was applied, or null if pending.
    /// </summary>
    public DateTimeOffset? AppliedOn { get; }

    private MigrationInfo(long version, string description, DateTimeOffset? appliedOn)
    {
        Version = version;
        Description = description;
        AppliedOn = appliedOn;
    }

    /// <summary>
    /// Creates a new <see cref="MigrationInfo"/> instance.
    /// </summary>
    /// <param name="version">Migration version number. Must be positive.</param>
    /// <param name="description">Human-readable description.</param>
    /// <param name="appliedOn">Timestamp when applied, or null if pending.</param>
    public static MigrationInfo Create(long version, string description, DateTimeOffset? appliedOn = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new MigrationInfo(version, description, appliedOn);
    }
}
