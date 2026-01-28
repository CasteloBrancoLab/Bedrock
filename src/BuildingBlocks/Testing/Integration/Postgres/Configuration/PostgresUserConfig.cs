using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;

namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;

/// <summary>
/// Immutable configuration for a PostgreSQL user with permissions.
/// </summary>
public sealed class PostgresUserConfig
{
    /// <summary>
    /// Gets the username.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the password.
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Gets the schema-level permissions.
    /// </summary>
    public IReadOnlyDictionary<string, PostgresSchemaPermission> SchemaPermissions { get; }

    /// <summary>
    /// Gets the database-specific permissions.
    /// </summary>
    public IReadOnlyDictionary<string, PostgresUserDatabasePermissionConfig> DatabasePermissions { get; }

    internal PostgresUserConfig(
        string username,
        string password,
        IReadOnlyDictionary<string, PostgresSchemaPermission> schemaPermissions,
        IReadOnlyDictionary<string, PostgresUserDatabasePermissionConfig> databasePermissions)
    {
        Username = username;
        Password = password;
        SchemaPermissions = schemaPermissions;
        DatabasePermissions = databasePermissions;
    }
}
