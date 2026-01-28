using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Configuration;
using Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Runtime;

/// <summary>
/// Wraps a running PostgreSQL container with its databases and users.
/// </summary>
public sealed class PostgresContainerWrapper : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;
    private readonly PostgresContainerConfig _config;
    private readonly Dictionary<string, PostgresDatabase> _databases = [];
    private readonly Dictionary<string, PostgresUser> _users = [];

    internal PostgresContainerWrapper(
        PostgreSqlContainer container,
        PostgresContainerConfig config)
    {
        _container = container;
        _config = config;
    }

    /// <summary>
    /// Gets a database by name.
    /// </summary>
    /// <param name="databaseName">The database name.</param>
    /// <returns>The database wrapper.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the database is not found.</exception>
    public PostgresDatabase this[string databaseName] =>
        _databases.TryGetValue(databaseName, out var db)
            ? db
            : throw new KeyNotFoundException($"Database '{databaseName}' not found. Available: {string.Join(", ", _databases.Keys)}");

    /// <summary>
    /// Gets all databases.
    /// </summary>
    public IReadOnlyDictionary<string, PostgresDatabase> Databases => _databases;

    /// <summary>
    /// Gets all users.
    /// </summary>
    public IReadOnlyDictionary<string, PostgresUser> Users => _users;

    /// <summary>
    /// Gets a connection string for a specific database and optional user.
    /// </summary>
    /// <param name="databaseName">The database name.</param>
    /// <param name="user">The username (optional, defaults to admin).</param>
    /// <returns>The connection string.</returns>
    public string GetConnectionString(string databaseName, string? user = null)
    {
        var database = this[databaseName];

        if (user is null)
        {
            return database.GetAdminConnectionString();
        }

        if (!_users.TryGetValue(user, out var userObj))
        {
            throw new KeyNotFoundException($"User '{user}' not found. Available: {string.Join(", ", _users.Keys)}");
        }

        return database.GetConnectionString(userObj.Username, userObj.Password);
    }

    /// <summary>
    /// Initializes the container by creating databases, users, and applying permissions.
    /// </summary>
    internal async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var host = _container.Hostname;
        var port = _container.GetMappedPublicPort(5432);
        const string adminPassword = "postgres";

        // Create databases
        foreach (var dbConfig in _config.Databases.Values)
        {
            await CreateDatabaseAsync(dbConfig.Name, cancellationToken);

            var database = new PostgresDatabase(dbConfig, host, port, adminPassword);
            _databases[dbConfig.Name] = database;

            // Run seeds
            await RunSeedsAsync(database, dbConfig, cancellationToken);
        }

        // Create users
        foreach (var userConfig in _config.Users.Values)
        {
            await CreateUserAsync(userConfig, cancellationToken);
            _users[userConfig.Username] = new PostgresUser(userConfig);
        }

        // Apply permissions (after all databases and users exist)
        foreach (var userConfig in _config.Users.Values)
        {
            await ApplyPermissionsAsync(userConfig, cancellationToken);
        }
    }

    private async Task CreateDatabaseAsync(string databaseName, CancellationToken cancellationToken)
    {
        var sql = $"CREATE DATABASE \"{databaseName}\"";
        await _container.ExecScriptAsync(sql, cancellationToken);
    }

    private async Task RunSeedsAsync(
        PostgresDatabase database,
        PostgresDatabaseConfig config,
        CancellationToken cancellationToken)
    {
        // Run script files
        foreach (var scriptPath in config.SeedScriptPaths)
        {
            var sql = await File.ReadAllTextAsync(scriptPath, cancellationToken);
            await ExecuteOnDatabaseAsync(database.Name, sql, cancellationToken);
        }

        // Run inline SQL
        foreach (var sql in config.SeedSqlStatements)
        {
            await ExecuteOnDatabaseAsync(database.Name, sql, cancellationToken);
        }
    }

    private async Task CreateUserAsync(PostgresUserConfig config, CancellationToken cancellationToken)
    {
        var sql = $"CREATE USER \"{config.Username}\" WITH PASSWORD '{config.Password}'";
        await _container.ExecScriptAsync(sql, cancellationToken);
    }

    private async Task ApplyPermissionsAsync(PostgresUserConfig config, CancellationToken cancellationToken)
    {
        // Schema permissions (apply on each database that user has access to)
        foreach (var (schema, permission) in config.SchemaPermissions)
        {
            var grants = BuildSchemaGrants(schema, config.Username, permission);
            foreach (var dbName in config.DatabasePermissions.Keys)
            {
                foreach (var grant in grants)
                {
                    await ExecuteOnDatabaseAsync(dbName, grant, cancellationToken);
                }
            }
        }

        // Database-specific permissions
        foreach (var (dbName, dbPermission) in config.DatabasePermissions)
        {
            // Grant CONNECT permission
            await _container.ExecScriptAsync(
                $"GRANT CONNECT ON DATABASE \"{dbName}\" TO \"{config.Username}\"",
                cancellationToken);

            // All tables permission
            if (dbPermission.AllTablesPermission.HasValue)
            {
                var grants = BuildAllTablesGrants(config.Username, dbPermission.AllTablesPermission.Value);
                foreach (var grant in grants)
                {
                    await ExecuteOnDatabaseAsync(dbName, grant, cancellationToken);
                }
            }

            // Specific table permissions
            foreach (var (table, permission) in dbPermission.TablePermissions)
            {
                var grants = BuildTableGrants(table, config.Username, permission);
                foreach (var grant in grants)
                {
                    await ExecuteOnDatabaseAsync(dbName, grant, cancellationToken);
                }
            }

            // All sequences permission
            if (dbPermission.AllSequencesPermission.HasValue)
            {
                var grants = BuildAllSequencesGrants(config.Username, dbPermission.AllSequencesPermission.Value);
                foreach (var grant in grants)
                {
                    await ExecuteOnDatabaseAsync(dbName, grant, cancellationToken);
                }
            }

            // Specific sequence permissions
            foreach (var (sequence, permission) in dbPermission.SequencePermissions)
            {
                var grants = BuildSequenceGrants(sequence, config.Username, permission);
                foreach (var grant in grants)
                {
                    await ExecuteOnDatabaseAsync(dbName, grant, cancellationToken);
                }
            }
        }
    }

    private async Task ExecuteOnDatabaseAsync(
        string databaseName,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(
            $"Host={_container.Hostname};Port={_container.GetMappedPublicPort(5432)};" +
            $"Database={databaseName};Username=postgres;Password=postgres");

        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static IEnumerable<string> BuildSchemaGrants(
        string schema,
        string user,
        PostgresSchemaPermission permission)
    {
        if (permission.HasFlag(PostgresSchemaPermission.Usage))
        {
            yield return $"GRANT USAGE ON SCHEMA \"{schema}\" TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresSchemaPermission.Create))
        {
            yield return $"GRANT CREATE ON SCHEMA \"{schema}\" TO \"{user}\"";
        }
    }

    private static IEnumerable<string> BuildTableGrants(
        string table,
        string user,
        PostgresTablePermission permission)
    {
        if (permission.HasFlag(PostgresTablePermission.Select))
        {
            yield return $"GRANT SELECT ON \"{table}\" TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.Insert))
        {
            yield return $"GRANT INSERT ON \"{table}\" TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.Update))
        {
            yield return $"GRANT UPDATE ON \"{table}\" TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.Delete))
        {
            yield return $"GRANT DELETE ON \"{table}\" TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.Truncate))
        {
            yield return $"GRANT TRUNCATE ON \"{table}\" TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.References))
        {
            yield return $"GRANT REFERENCES ON \"{table}\" TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.Trigger))
        {
            yield return $"GRANT TRIGGER ON \"{table}\" TO \"{user}\"";
        }
    }

    private static IEnumerable<string> BuildAllTablesGrants(
        string user,
        PostgresTablePermission permission)
    {
        if (permission.HasFlag(PostgresTablePermission.Select))
        {
            yield return $"GRANT SELECT ON ALL TABLES IN SCHEMA public TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.Insert))
        {
            yield return $"GRANT INSERT ON ALL TABLES IN SCHEMA public TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.Update))
        {
            yield return $"GRANT UPDATE ON ALL TABLES IN SCHEMA public TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.Delete))
        {
            yield return $"GRANT DELETE ON ALL TABLES IN SCHEMA public TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.Truncate))
        {
            yield return $"GRANT TRUNCATE ON ALL TABLES IN SCHEMA public TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.References))
        {
            yield return $"GRANT REFERENCES ON ALL TABLES IN SCHEMA public TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresTablePermission.Trigger))
        {
            yield return $"GRANT TRIGGER ON ALL TABLES IN SCHEMA public TO \"{user}\"";
        }

        // Set default privileges for future tables
        var privilegesList = GetTablePrivilegesList(permission);
        if (!string.IsNullOrEmpty(privilegesList))
        {
            yield return $"ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT {privilegesList} ON TABLES TO \"{user}\"";
        }
    }

    private static IEnumerable<string> BuildSequenceGrants(
        string sequence,
        string user,
        PostgresSequencePermission permission)
    {
        if (permission.HasFlag(PostgresSequencePermission.Usage))
        {
            yield return $"GRANT USAGE ON SEQUENCE \"{sequence}\" TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresSequencePermission.Select))
        {
            yield return $"GRANT SELECT ON SEQUENCE \"{sequence}\" TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresSequencePermission.Update))
        {
            yield return $"GRANT UPDATE ON SEQUENCE \"{sequence}\" TO \"{user}\"";
        }
    }

    private static IEnumerable<string> BuildAllSequencesGrants(
        string user,
        PostgresSequencePermission permission)
    {
        if (permission.HasFlag(PostgresSequencePermission.Usage))
        {
            yield return $"GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresSequencePermission.Select))
        {
            yield return $"GRANT SELECT ON ALL SEQUENCES IN SCHEMA public TO \"{user}\"";
        }

        if (permission.HasFlag(PostgresSequencePermission.Update))
        {
            yield return $"GRANT UPDATE ON ALL SEQUENCES IN SCHEMA public TO \"{user}\"";
        }
    }

    private static string GetTablePrivilegesList(PostgresTablePermission permission)
    {
        var privileges = new List<string>();

        if (permission.HasFlag(PostgresTablePermission.Select))
        {
            privileges.Add("SELECT");
        }

        if (permission.HasFlag(PostgresTablePermission.Insert))
        {
            privileges.Add("INSERT");
        }

        if (permission.HasFlag(PostgresTablePermission.Update))
        {
            privileges.Add("UPDATE");
        }

        if (permission.HasFlag(PostgresTablePermission.Delete))
        {
            privileges.Add("DELETE");
        }

        if (permission.HasFlag(PostgresTablePermission.Truncate))
        {
            privileges.Add("TRUNCATE");
        }

        if (permission.HasFlag(PostgresTablePermission.References))
        {
            privileges.Add("REFERENCES");
        }

        if (permission.HasFlag(PostgresTablePermission.Trigger))
        {
            privileges.Add("TRIGGER");
        }

        return string.Join(", ", privileges);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
