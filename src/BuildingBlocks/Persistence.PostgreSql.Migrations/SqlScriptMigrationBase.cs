using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;

/// <summary>
/// Base class for SQL-script-driven migrations.
/// Developers inherit from this class and decorate with
/// [Migration(version)] and [SqlScript("Up/...", "Down/...")].
/// The class body is empty — all logic is in the base.
/// </summary>
public abstract class SqlScriptMigrationBase : FluentMigrator.Migration
{
    private readonly string _upScriptResourceName;
    private readonly string? _downScriptResourceName;

    /// <summary>
    /// Initializes a new instance of <see cref="SqlScriptMigrationBase"/>.
    /// Reads the <see cref="SqlScriptAttribute"/> from the derived class and validates
    /// that the referenced embedded scripts exist in the assembly.
    /// </summary>
    protected SqlScriptMigrationBase()
    {
        var attribute = GetType().GetCustomAttribute<SqlScriptAttribute>()
            ?? throw new InvalidOperationException(
                $"Migration class '{GetType().Name}' must be decorated with [{nameof(SqlScriptAttribute)}].");

        // Stryker disable all : Normalizacao e validacao de scripts embarcados - coberto por testes de integracao
        _upScriptResourceName = NormalizeResourceName(attribute.UpScriptResourceName);
        _downScriptResourceName = attribute.DownScriptResourceName is not null
            ? NormalizeResourceName(attribute.DownScriptResourceName)
            : null;

        ValidateScriptExists(_upScriptResourceName, "UP");

        if (_downScriptResourceName is not null)
            ValidateScriptExists(_downScriptResourceName, "DOWN");
        // Stryker restore all
    }

    /// <summary>
    /// Executes the UP embedded SQL script.
    /// </summary>
    // Stryker disable once all : Requer contexto FluentMigrator - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer contexto FluentMigrator - coberto por testes de integracao")]
    public override void Up()
    {
        Execute.EmbeddedScript(_upScriptResourceName);
    }

    /// <summary>
    /// Executes the DOWN embedded SQL script.
    /// Throws <see cref="InvalidOperationException"/> if no DOWN script was specified (irreversible migration).
    /// </summary>
    // Stryker disable once all : Requer contexto FluentMigrator - coberto por testes de integracao
    [ExcludeFromCodeCoverage(Justification = "Requer contexto FluentMigrator - coberto por testes de integracao")]
    public override void Down()
    {
        if (_downScriptResourceName is null)
            throw new InvalidOperationException(
                $"Migration '{GetType().Name}' does not have a DOWN script. This is an irreversible migration.");

        Execute.EmbeddedScript(_downScriptResourceName);
    }

    [ExcludeFromCodeCoverage(Justification = "Normalizacao de path separators - coberto por testes de integracao")]
    private static string NormalizeResourceName(string resourceName)
    {
        return resourceName.Replace('/', '.').Replace('\\', '.');
    }

    [ExcludeFromCodeCoverage(Justification = "Validacao de script embarcado - coberto por testes de integracao")]
    private void ValidateScriptExists(string normalizedResourceName, string direction)
    {
        var assembly = GetType().Assembly;

        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (name.EndsWith(normalizedResourceName, StringComparison.OrdinalIgnoreCase))
                return;
        }

        throw new InvalidOperationException(
            $"Embedded {direction} script '{normalizedResourceName}' not found in assembly '{assembly.GetName().Name}'.");
    }
}
