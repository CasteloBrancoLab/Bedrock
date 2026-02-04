using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-026: Propriedades derivadas devem ser <b>persistidas</b> (armazenadas),
/// nunca calculadas via expression body (<c>=&gt;</c>).
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Propriedades públicas de instância NÃO devem usar expression body (<c>=&gt;</c>)</item>
///   <item>Devem usar <c>{ get; private set; }</c> para garantir preservação histórica</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Propriedades estáticas (ex: metadados)</item>
///   <item>Propriedades definidas na classe base (herdadas de EntityBase)</item>
///   <item>Propriedades de coleções com full property block (ex: IReadOnlyList via AsReadOnly)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE026_DerivedPropertiesStoredRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE026_DerivedPropertiesStored";

    public override string Description =>
        "Propriedades públicas de instância devem ser persistidas, não calculadas via expression body (DE-026)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-026-propriedades-derivadas-persistidas-vs-calculadas.md";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            // Apenas propriedades públicas
            if (property.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Ignorar propriedades estáticas (metadados)
            if (property.IsStatic)
                continue;

            // Ignorar propriedades herdadas (definidas em classe base)
            if (!SymbolEqualityComparer.Default.Equals(property.ContainingType, type))
                continue;

            // Ignorar overrides (propriedades da base que são sobrescritas)
            if (property.IsOverride)
                continue;

            // Verificar se usa expression body
            if (HasExpressionBody(property))
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetPropertyLineNumber(property, context.LineNumber),
                    Message = $"A propriedade pública '{property.Name}' da classe '{type.Name}' usa " +
                              $"expression body (=>). Propriedades derivadas devem ser armazenadas " +
                              $"com '{{ get; private set; }}' para preservar integridade histórica, " +
                              $"auditoria e reconstitution correto",
                    LlmHint = $"Refatorar '{property.Name}' de '{property.Type} {property.Name} => ...' " +
                              $"para '{property.Type} {property.Name} {{ get; private set; }}' e " +
                              $"atualizar o valor via Set* nos métodos *Internal. " +
                              $"Consultar ADR DE-026 para exemplos de uso correto"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se a propriedade usa expression body (=>).
    /// </summary>
    private static bool HasExpressionBody(IPropertySymbol property)
    {
        foreach (var syntaxRef in property.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            if (syntaxNode is PropertyDeclarationSyntax propertyDeclaration &&
                propertyDeclaration.ExpressionBody is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Obtém o número da linha onde a propriedade é declarada.
    /// </summary>
    private static int GetPropertyLineNumber(IPropertySymbol property, int fallbackLineNumber)
    {
        var location = property.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
