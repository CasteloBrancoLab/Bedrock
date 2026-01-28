namespace Bedrock.BuildingBlocks.Testing.Integration.Postgres.Permissions;

/// <summary>
/// Represents PostgreSQL sequence-level permissions.
/// </summary>
[Flags]
public enum PostgresSequencePermission
{
    /// <summary>
    /// No permissions.
    /// </summary>
    None = 0,

    /// <summary>
    /// USAGE permission - Allows NEXTVAL and CURRVAL.
    /// </summary>
    Usage = 1,

    /// <summary>
    /// SELECT permission - Allows querying current value.
    /// </summary>
    Select = 2,

    /// <summary>
    /// UPDATE permission - Allows SETVAL.
    /// </summary>
    Update = 4,

    /// <summary>
    /// All sequence permissions.
    /// </summary>
    All = Usage | Select | Update
}
