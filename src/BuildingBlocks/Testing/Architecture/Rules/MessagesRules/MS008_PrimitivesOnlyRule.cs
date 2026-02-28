using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-008: Parametros de mensagens concretas (exceto Metadata) devem usar
/// apenas tipos primitivos, readonly record structs, enums, ou colecoes readonly desses.
/// Tipos de dominio sao proibidos.
/// </summary>
public sealed class MS008_PrimitivesOnlyRule : MessageRuleBase
{
    public override string Name => "MS008_PrimitivesOnly";

    public override string Description =>
        "Parametros de mensagens devem usar apenas primitivos, record structs, enums ou colecoes readonly — sem tipos de dominio (MS-008)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-008-message-models-primitivos-sem-tipos-dominio.md";

    /// <summary>
    /// Namespaces proibidos que indicam tipos de dominio.
    /// </summary>
    private static readonly string[] ForbiddenNamespaceFragments =
        [".Domain.Entities", ".Domain.Services", "Bedrock.BuildingBlocks.Core"];

    /// <summary>
    /// Tipos primitivos permitidos (por nome simples).
    /// </summary>
    private static readonly HashSet<string> AllowedPrimitiveNames = new(StringComparer.Ordinal)
    {
        "Guid", "String", "DateTimeOffset", "DateTime",
        "Int32", "Int64", "Int16", "Byte",
        "Decimal", "Single", "Double", "Char", "Boolean",
        "Nullable"
    };

    protected override Violation? AnalyzeMessageType(TypeContext context)
    {
        var type = context.Type;

        // Obter o construtor primario (parametros posicionais do record)
        var primaryCtor = type.InstanceConstructors
            .FirstOrDefault(c => c.Parameters.Length > 0 && !c.IsImplicitlyDeclared);

        primaryCtor ??= type.InstanceConstructors
            .Where(c => c.Parameters.Length > 0)
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();

        if (primaryCtor is null)
            return null;

        // Verificar todos os parametros exceto o primeiro (Metadata)
        foreach (var param in primaryCtor.Parameters.Skip(1))
        {
            if (!IsAllowedType(param.Type))
            {
                var paramTypeDisplay = param.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = context.LineNumber,
                    Message = $"O parametro '{param.Name}' de '{type.Name}' usa tipo '{paramTypeDisplay}' " +
                              $"que nao e primitivo nem readonly record struct. Mensagens devem usar apenas tipos serializaveis primitivos",
                    LlmHint = $"Substituir o tipo '{paramTypeDisplay}' do parametro '{param.Name}' por um " +
                              $"tipo primitivo (Guid, string, etc.) ou um readonly record struct com primitivos"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se um tipo e permitido como parametro de mensagem.
    /// </summary>
    private static bool IsAllowedType(ITypeSymbol typeSymbol)
    {
        // Desembrulhar Nullable<T>
        if (typeSymbol is INamedTypeSymbol { IsGenericType: true, Name: "Nullable" } nullable)
        {
            return IsAllowedType(nullable.TypeArguments[0]);
        }

        // Verificar namespace proibido
        var fullNamespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        foreach (var forbidden in ForbiddenNamespaceFragments)
        {
            if (fullNamespace.Contains(forbidden, StringComparison.Ordinal))
                return false;
        }

        // Primitivos (por SpecialType ou nome)
        if (typeSymbol.SpecialType != SpecialType.None && typeSymbol.SpecialType != SpecialType.System_Object)
            return true;

        if (AllowedPrimitiveNames.Contains(typeSymbol.Name))
            return true;

        // Enums com tipo subjacente primitivo
        if (typeSymbol.TypeKind == TypeKind.Enum)
            return true;

        // Readonly record struct (message models/DTOs)
        if (typeSymbol is INamedTypeSymbol namedType)
        {
            if (namedType.IsReadOnly && namedType.IsRecord && namedType.IsValueType)
                return true;

            // IReadOnlyList<T> / IReadOnlyCollection<T>
            if (namedType.IsGenericType &&
                namedType.Name is "IReadOnlyList" or "IReadOnlyCollection")
            {
                return namedType.TypeArguments.All(IsAllowedType);
            }
        }

        return false;
    }
}
