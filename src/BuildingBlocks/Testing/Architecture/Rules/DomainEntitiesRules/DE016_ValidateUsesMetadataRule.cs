using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-016: Métodos <c>Validate*</c> devem referenciar metadados da classe
/// <c>{EntityName}Metadata</c> (Single Source of Truth), não usar valores hardcoded.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Métodos <c>Validate*</c> (públicos e estáticos) devem referenciar pelo menos
///         um membro da classe <c>{EntityName}Metadata</c></item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Método <c>IsValid</c> (orchestrator que delega para <c>Validate*</c>)</item>
///   <item>Método <c>IsValidInternal</c> (override de EntityBase)</item>
///   <item>Métodos <c>Validate*Internal</c> (helpers privados de validação por operação)</item>
///   <item>Entidades sem classe de metadados aninhada</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE016_ValidateUsesMetadataRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE016_ValidateUsesMetadata";

    public override string Description =>
        "Métodos Validate* devem referenciar metadados da classe Metadata como Single Source of Truth (DE-016)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-016-single-source-of-truth-para-regras-de-validacao.md";

    /// <summary>
    /// Sufixo obrigatório da classe de metadados aninhada.
    /// </summary>
    private const string MetadataSuffix = "Metadata";

    /// <summary>
    /// Nome do método orchestrator que é exceção (não precisa referenciar metadata diretamente).
    /// </summary>
    private const string IsValidMethodName = "IsValid";

    /// <summary>
    /// Nome do método protegido de EntityBase que é exceção.
    /// </summary>
    private const string IsValidInternalMethodName = "IsValidInternal";

    /// <summary>
    /// Sufixo de métodos *Internal que são helpers privados.
    /// </summary>
    private const string InternalSuffix = "Internal";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Buscar a classe de metadados aninhada: {EntityName}Metadata
        var expectedMetadataName = type.Name + MetadataSuffix;
        INamedTypeSymbol? metadataClass = null;

        foreach (var nestedType in type.GetTypeMembers())
        {
            if (nestedType.Name == expectedMetadataName && nestedType.IsStatic)
            {
                metadataClass = nestedType;
                break;
            }
        }

        // Se não há classe de metadados, não há o que verificar nesta regra
        if (metadataClass is null)
            return null;

        // Verificar cada método Validate* público e estático
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Ignorar métodos sem corpo
            if (method.IsAbstract || method.IsExtern)
                continue;

            // Ignorar métodos herdados de object
            if (IsObjectMethod(method))
                continue;

            // Ignorar property accessors, operators, etc.
            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Ignorar IsValid (orchestrator) e IsValidInternal
            if (method.Name == IsValidMethodName || method.Name == IsValidInternalMethodName)
                continue;

            // Verificar apenas métodos de validação (Validate*)
            if (!method.Name.StartsWith("Validate", StringComparison.Ordinal))
                continue;

            // Ignorar métodos Validate*Internal (helpers privados)
            if (method.Name.EndsWith(InternalSuffix, StringComparison.Ordinal))
                continue;

            // Verificar apenas métodos públicos e estáticos
            if (!method.IsStatic || method.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Verificar se o método referencia a classe de metadados
            if (!ReferencesMetadataClass(method, expectedMetadataName))
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMemberLineNumber(method, context.LineNumber),
                    Message = $"O método '{method.Name}' da classe '{type.Name}' não referencia " +
                              $"'{expectedMetadataName}'. Métodos Validate* devem usar metadados como " +
                              $"Single Source of Truth, não valores hardcoded",
                    LlmHint = $"Substituir valores hardcoded no método '{method.Name}' da classe '{type.Name}' " +
                              $"por referências a '{expectedMetadataName}.*'. " +
                              $"Exemplo: usar '{expectedMetadataName}.FirstNameMaxLength' ao invés de um literal numérico. " +
                              $"Consultar ADR DE-016 para exemplos de uso correto"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o corpo do método referencia a classe de metadados via syntax tree.
    /// Procura por <c>MemberAccessExpressionSyntax</c> cujo lado esquerdo é
    /// um <c>IdentifierNameSyntax</c> com o nome da classe de metadados.
    /// </summary>
    private static bool ReferencesMetadataClass(IMethodSymbol method, string metadataClassName)
    {
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression is IdentifierNameSyntax identifier &&
                    identifier.Identifier.ValueText == metadataClassName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Obtém o número da linha onde o membro é declarado, ou fallback para a linha da classe.
    /// </summary>
    private static int GetMemberLineNumber(ISymbol member, int fallbackLineNumber)
    {
        var location = member.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
