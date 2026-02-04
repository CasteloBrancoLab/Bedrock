using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-012: Entidades de domínio devem usar metadados estáticos ao invés de Data Annotations.
/// Data Annotations exigem reflexão, são incompatíveis com AOT, forçam valores compile-time
/// e causam duplicação entre camadas.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Propriedades de entidades NÃO devem ter atributos de <c>System.ComponentModel.DataAnnotations</c></item>
///   <item>Campos de entidades NÃO devem ter atributos de <c>System.ComponentModel.DataAnnotations</c></item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Atributos de outros namespaces (ex: <c>System.Text.Json</c>)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE012_StaticMetadataOverDataAnnotationsRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE012_StaticMetadataOverDataAnnotations";

    public override string Description =>
        "Entidades de domínio devem usar metadados estáticos ao invés de Data Annotations (DE-012)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-012-metadados-estaticos-vs-data-annotations.md";

    /// <summary>
    /// Namespace de Data Annotations que não deve ser usado em entidades de domínio.
    /// </summary>
    private const string DataAnnotationsNamespace = "System.ComponentModel.DataAnnotations";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Verificar propriedades
        foreach (var member in type.GetMembers())
        {
            if (member is IPropertySymbol property)
            {
                var violation = CheckMemberForDataAnnotations(property, property.GetAttributes(), type, context);
                if (violation is not null)
                    return violation;
            }
            else if (member is IFieldSymbol field)
            {
                // Ignorar backing fields gerados pelo compilador
                if (field.IsImplicitlyDeclared)
                    continue;

                var violation = CheckMemberForDataAnnotations(field, field.GetAttributes(), type, context);
                if (violation is not null)
                    return violation;
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se um membro (propriedade ou campo) possui atributos de Data Annotations.
    /// </summary>
    private Violation? CheckMemberForDataAnnotations(
        ISymbol member,
        System.Collections.Immutable.ImmutableArray<AttributeData> attributes,
        INamedTypeSymbol type,
        TypeContext context)
    {
        foreach (var attribute in attributes)
        {
            if (attribute.AttributeClass is null)
                continue;

            var attributeNamespace = attribute.AttributeClass.ContainingNamespace?.ToDisplayString();

            if (attributeNamespace is null)
                continue;

            // Verificar se o atributo pertence ao namespace de Data Annotations
            // ou a um sub-namespace (ex: System.ComponentModel.DataAnnotations.Schema)
            if (attributeNamespace.StartsWith(DataAnnotationsNamespace, StringComparison.Ordinal))
            {
                var attributeName = attribute.AttributeClass.Name;
                var memberKind = member is IPropertySymbol ? "propriedade" : "campo";

                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMemberLineNumber(member, context.LineNumber),
                    Message = $"A {memberKind} '{member.Name}' da classe '{type.Name}' usa o atributo " +
                              $"[{attributeName}] de Data Annotations. " +
                              $"Entidades de domínio devem usar metadados estáticos ({type.Name}Metadata) " +
                              $"ao invés de Data Annotations",
                    LlmHint = $"Remover o atributo [{attributeName}] da {memberKind} '{member.Name}' " +
                              $"da classe '{type.Name}' e usar a classe aninhada estática " +
                              $"'{type.Name}Metadata' para expor metadados de validação. " +
                              $"Consultar ADR DE-012 para exemplos de uso correto"
                };
            }
        }

        return null;
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
