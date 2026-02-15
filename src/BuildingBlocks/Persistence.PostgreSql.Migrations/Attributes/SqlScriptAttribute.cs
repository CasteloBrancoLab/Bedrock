namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;

/// <summary>
/// Links a migration class to its embedded UP and DOWN SQL script resource names.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class SqlScriptAttribute : Attribute
{
    /// <summary>
    /// Gets the relative resource name of the UP migration script.
    /// </summary>
    public string UpScriptResourceName { get; }

    /// <summary>
    /// Gets the relative resource name of the DOWN migration script.
    /// Null for irreversible migrations.
    /// </summary>
    public string? DownScriptResourceName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlScriptAttribute"/> class.
    /// </summary>
    /// <param name="upScriptResourceName">Relative resource name for the UP script (e.g., "Up/V202602141200__create_users.sql").</param>
    /// <param name="downScriptResourceName">Relative resource name for the DOWN script, or null for irreversible migrations.</param>
    public SqlScriptAttribute(string upScriptResourceName, string? downScriptResourceName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(upScriptResourceName);

        UpScriptResourceName = upScriptResourceName;
        DownScriptResourceName = downScriptResourceName;
    }
}
