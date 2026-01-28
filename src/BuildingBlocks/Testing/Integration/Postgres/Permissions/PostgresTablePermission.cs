namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;

/// <summary>
/// Represents PostgreSQL table-level permissions.
/// Maps to operations used by Persistence.PostgreSql repository methods.
/// </summary>
[Flags]
public enum PostgresTablePermission
{
    /// <summary>
    /// No permissions.
    /// </summary>
    None = 0,

    /// <summary>
    /// SELECT permission - Used by GetByIdAsync, EnumerateAllAsync, ExistsAsync.
    /// </summary>
    Select = 1,

    /// <summary>
    /// INSERT permission - Used by InsertAsync, COPY (BinaryImporter).
    /// </summary>
    Insert = 2,

    /// <summary>
    /// UPDATE permission - Used by UpdateAsync.
    /// </summary>
    Update = 4,

    /// <summary>
    /// DELETE permission - Used by DeleteAsync.
    /// </summary>
    Delete = 8,

    /// <summary>
    /// TRUNCATE permission - Useful for test cleanup.
    /// </summary>
    Truncate = 16,

    /// <summary>
    /// REFERENCES permission - Foreign key creation.
    /// </summary>
    References = 32,

    /// <summary>
    /// TRIGGER permission - Trigger management.
    /// </summary>
    Trigger = 64,

    /// <summary>
    /// Read-only access (SELECT only).
    /// </summary>
    ReadOnly = Select,

    /// <summary>
    /// Read-write access (SELECT, INSERT, UPDATE, DELETE).
    /// </summary>
    ReadWrite = Select | Insert | Update | Delete,

    /// <summary>
    /// All permissions.
    /// </summary>
    All = Select | Insert | Update | Delete | Truncate | References | Trigger
}
