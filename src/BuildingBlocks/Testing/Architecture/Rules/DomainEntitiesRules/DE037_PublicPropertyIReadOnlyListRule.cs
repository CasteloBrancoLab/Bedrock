using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-037: Propriedades publicas de colecao devem retornar
/// <c>IReadOnlyList&lt;T&gt;</c> via <c>AsReadOnly()</c>.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Propriedades publicas de colecao NAO devem expor tipos mutaveis
///         (<c>List&lt;T&gt;</c>, <c>IList&lt;T&gt;</c>, <c>ICollection&lt;T&gt;</c>)</item>
///   <item>Propriedades publicas de colecao NAO devem expor <c>IEnumerable&lt;T&gt;</c>
///         (API pobre, sem Count/indexador)</item>
///   <item>Propriedades publicas que retornam <c>IReadOnlyList&lt;T&gt;</c> devem
///         usar <c>AsReadOnly()</c> no corpo (nao <c>ToList()</c>)</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Propriedades estaticas</item>
///   <item>Propriedades herdadas ou override</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE037_PublicPropertyIReadOnlyListRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE037_PublicPropertyIReadOnlyList";

    public override string Description =>
        "Propriedades publicas de colecao devem retornar IReadOnlyList<T> via AsReadOnly() (DE-037)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-037-propriedade-publica-ireadonlylist-asreadonly.md";

    /// <summary>
    /// Nomes de tipos de colecao que NAO devem ser usados em propriedades publicas.
    /// </summary>
    private static readonly HashSet<string> ForbiddenCollectionTypeNames = new(StringComparer.Ordinal)
    {
        "List",
        "IList",
        "ICollection",
        "Collection",
        "HashSet",
        "ISet",
        "IEnumerable"
    };

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            // Apenas propriedades publicas
            if (property.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Ignorar estaticas
            if (property.IsStatic)
                continue;

            // Ignorar herdadas
            if (!SymbolEqualityComparer.Default.Equals(property.ContainingType, type))
                continue;

            // Ignorar overrides
            if (property.IsOverride)
                continue;

            // Verificar se o tipo de retorno e uma colecao proibida
            if (property.Type is INamedTypeSymbol namedType &&
                namedType.IsGenericType &&
                ForbiddenCollectionTypeNames.Contains(namedType.Name))
            {
                var typeArg = namedType.TypeArguments.Length > 0
                    ? namedType.TypeArguments[0].Name
                    : "T";

                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetPropertyLineNumber(property, context.LineNumber),
                    Message = $"A propriedade publica '{property.Name}' da classe " +
                              $"'{type.Name}' retorna '{namedType.Name}<{typeArg}>'. " +
                              $"Propriedades publicas de colecao devem retornar " +
                              $"IReadOnlyList<{typeArg}> via AsReadOnly() para " +
                              $"garantir encapsulamento",
                    LlmHint = $"Alterar o tipo de retorno da propriedade '{property.Name}' " +
                              $"para IReadOnlyList<{typeArg}> e usar " +
                              $"_field.AsReadOnly() no getter. " +
                              $"Consultar ADR DE-037 para exemplos"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Obtem o numero da linha onde a propriedade e declarada.
    /// </summary>
    private static int GetPropertyLineNumber(IPropertySymbol property, int fallbackLineNumber)
    {
        var location = property.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
