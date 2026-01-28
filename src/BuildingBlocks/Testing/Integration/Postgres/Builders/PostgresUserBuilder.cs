using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;

namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Builders;

/// <summary>
/// Fluent builder for PostgreSQL user configuration.
/// </summary>
public sealed class PostgresUserBuilder
{
    private readonly string _username;
    private readonly string _password;
    private readonly Dictionary<string, PostgresSchemaPermission> _schemaPermissions = [];
    private readonly Dictionary<string, PostgresUserDatabasePermissionBuilder> _databaseBuilders = [];

    internal PostgresUserBuilder(string username, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        _username = username;
        _password = password;
    }

    /// <summary>
    /// Sets schema-level permissions.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="permission">The permissions to grant.</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresUserBuilder WithSchemaPermission(
        string schemaName,
        PostgresSchemaPermission permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);
        _schemaPermissions[schemaName] = permission;
        return this;
    }

    /// <summary>
    /// Configures permissions for a specific database.
    /// </summary>
    /// <param name="databaseName">The database name.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>This builder for method chaining.</returns>
    public PostgresUserBuilder OnDatabase(
        string databaseName,
        Action<PostgresUserDatabasePermissionBuilder> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentNullException.ThrowIfNull(configure);

        if (!_databaseBuilders.TryGetValue(databaseName, out var builder))
        {
            builder = new PostgresUserDatabasePermissionBuilder(databaseName);
            _databaseBuilders[databaseName] = builder;
        }

        configure(builder);
        return this;
    }

    internal PostgresUserConfig Build()
    {
        var databasePermissions = _databaseBuilders
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Build());

        return new PostgresUserConfig(
            _username,
            _password,
            _schemaPermissions.AsReadOnly(),
            databasePermissions.AsReadOnly());
    }
}
