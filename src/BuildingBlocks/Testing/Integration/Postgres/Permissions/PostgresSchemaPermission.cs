namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;

/// <summary>
/// Represents PostgreSQL schema-level permissions.
/// </summary>
[Flags]
public enum PostgresSchemaPermission
{
    /// <summary>
    /// No permissions.
    /// </summary>
    None = 0,

    /// <summary>
    /// USAGE permission - Allows access to objects in the schema.
    /// </summary>
    Usage = 1,

    /// <summary>
    /// CREATE permission - Allows creating objects in the schema.
    /// </summary>
    Create = 2,

    /// <summary>
    /// All schema permissions.
    /// </summary>
    All = Usage | Create
}
