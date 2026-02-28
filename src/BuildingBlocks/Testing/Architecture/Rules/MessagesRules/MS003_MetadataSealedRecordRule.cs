using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-003: MessageMetadata deve ser sealed record com apenas tipos primitivos.
/// </summary>
public sealed class MS003_MetadataSealedRecordRule : MessageGeneralRuleBase
{
    public override string Name => "MS003_MetadataSealedRecord";

    public override string Description =>
        "MessageMetadata deve ser sealed record com apenas tipos primitivos (MS-003)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-003-message-metadata-sealed-record.md";

    /// <summary>
    /// Tipos primitivos permitidos nos parametros de MessageMetadata.
    /// </summary>
    private static readonly HashSet<string> AllowedPrimitiveNames = new(StringComparer.Ordinal)
    {
        "Guid", "String", "DateTimeOffset", "DateTime",
        "Int32", "Int64", "Int16", "Byte",
        "Decimal", "Single", "Double", "Char", "Boolean"
    };

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (type.Name != "MessageMetadata")
            return null;

        // Verificar: deve ser sealed
        if (!type.IsSealed)
        {
            return CreateViolation(context,
                "MessageMetadata deve ser sealed",
                "Adicionar modificador 'sealed' na declaracao de MessageMetadata");
        }

        // Verificar: deve ser record
        if (!type.IsRecord)
        {
            return CreateViolation(context,
                "MessageMetadata deve ser record",
                "Alterar MessageMetadata para 'sealed record'");
        }

        // Verificar: todos os parametros devem ser primitivos
        var primaryCtor = type.InstanceConstructors
            .FirstOrDefault(c => c.Parameters.Length > 0 && !c.IsImplicitlyDeclared);

        primaryCtor ??= type.InstanceConstructors
            .Where(c => c.Parameters.Length > 0)
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();

        if (primaryCtor is not null)
        {
            foreach (var param in primaryCtor.Parameters)
            {
                if (!IsPrimitiveType(param.Type))
                {
                    var paramTypeDisplay = param.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                    return CreateViolation(context,
                        $"O parametro '{param.Name}' de MessageMetadata usa tipo '{paramTypeDisplay}' que nao e primitivo",
                        $"Substituir '{paramTypeDisplay}' por um tipo primitivo (Guid, string, DateTimeOffset, etc.)");
                }
            }
        }

        return null;
    }

    private static bool IsPrimitiveType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.SpecialType != SpecialType.None && typeSymbol.SpecialType != SpecialType.System_Object)
            return true;

        return AllowedPrimitiveNames.Contains(typeSymbol.Name);
    }

    private Violation CreateViolation(TypeContext context, string message, string llmHint)
    {
        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = message,
            LlmHint = llmHint
        };
    }
}
