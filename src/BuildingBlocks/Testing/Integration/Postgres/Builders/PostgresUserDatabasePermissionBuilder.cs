using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;

namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Builders;

/// <summary>
/// Fluent builder for user permissions on a specific database.
/// </summary>
public sealed class PostgresUserDatabasePermissionBuilder
{
    private readonly string _databaseName;
    private readonly Dictionary<string, PostgresTablePermission> _tablePermissions = [];
    private PostgresTablePermission? _allTablesPermission;
    private readonly Dictionary<string, PostgresSequencePermission> _sequencePermissions = [];
    private PostgresSequencePermission? _allSequencesPermission;

    internal PostgresUserDatabasePermissionBuilder(string databaseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        _databaseName = databaseName;
    }

    /// <summary>
    /// Sets permissions for a specific table.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="permission">The permissions to grant.</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresUserDatabasePermissionBuilder OnTable(
        string tableName,
        PostgresTablePermission permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        _tablePermissions[tableName] = permission;
        return this;
    }

    /// <summary>
    /// Sets permissions for all tables in the database.
    /// </summary>
    /// <param name="permission">The permissions to grant to all tables.</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresUserDatabasePermissionBuilder OnAllTables(
        PostgresTablePermission permission)
    {
        _allTablesPermission = permission;
        return this;
    }

    /// <summary>
    /// Sets permissions for a specific sequence.
    /// </summary>
    /// <param name="sequenceName">The sequence name.</param>
    /// <param name="permission">The permissions to grant.</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresUserDatabasePermissionBuilder OnSequence(
        string sequenceName,
        PostgresSequencePermission permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sequenceName);
        _sequencePermissions[sequenceName] = permission;
        return this;
    }

    /// <summary>
    /// Sets permissions for all sequences in the database.
    /// </summary>
    /// <param name="permission">The permissions to grant to all sequences.</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresUserDatabasePermissionBuilder OnAllSequences(
        PostgresSequencePermission permission)
    {
        _allSequencesPermission = permission;
        return this;
    }

    internal PostgresUserDatabasePermissionConfig Build()
    {
        return new PostgresUserDatabasePermissionConfig(
            _databaseName,
            _tablePermissions.AsReadOnly(),
            _allTablesPermission,
            _sequencePermissions.AsReadOnly(),
            _allSequencesPermission);
    }
}
