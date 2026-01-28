using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;

namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;

/// <summary>
/// Permission configuration for a specific database.
/// </summary>
public sealed class PostgresUserDatabasePermissionConfig
{
    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string DatabaseName { get; }

    /// <summary>
    /// Gets the table-specific permissions.
    /// </summary>
    public IReadOnlyDictionary<string, PostgresTablePermission> TablePermissions { get; }

    /// <summary>
    /// Gets the permission to apply to all tables, if set.
    /// </summary>
    public PostgresTablePermission? AllTablesPermission { get; }

    /// <summary>
    /// Gets the sequence-specific permissions.
    /// </summary>
    public IReadOnlyDictionary<string, PostgresSequencePermission> SequencePermissions { get; }

    /// <summary>
    /// Gets the permission to apply to all sequences, if set.
    /// </summary>
    public PostgresSequencePermission? AllSequencesPermission { get; }

    internal PostgresUserDatabasePermissionConfig(
        string databaseName,
        IReadOnlyDictionary<string, PostgresTablePermission> tablePermissions,
        PostgresTablePermission? allTablesPermission,
        IReadOnlyDictionary<string, PostgresSequencePermission> sequencePermissions,
        PostgresSequencePermission? allSequencesPermission)
    {
        DatabaseName = databaseName;
        TablePermissions = tablePermissions;
        AllTablesPermission = allTablesPermission;
        SequencePermissions = sequencePermissions;
        AllSequencesPermission = allSequencesPermission;
    }
}
