using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-036: Colecoes de entidades filhas devem ser encapsuladas como
/// <c>private List&lt;T&gt;</c> (field privado). A Aggregate Root deve controlar
/// todas as modificacoes na colecao via metodos de negocio.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Fields de colecao (<c>List&lt;T&gt;</c>, <c>IList&lt;T&gt;</c>,
///         <c>ICollection&lt;T&gt;</c>) NAO devem ser publicos ou internos</item>
///   <item>Propriedades de colecao mutavel NAO devem ser publicas</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Fields/propriedades estaticos</item>
///   <item>Propriedades <c>IReadOnlyList&lt;T&gt;</c> (exposicao correta)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE036_ChildCollectionPrivateListFieldRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE036_ChildCollectionPrivateListField";

    public override string Description =>
        "Colecoes de entidades filhas devem ser field privado List<T> (DE-036)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-036-colecoes-entidades-filhas-field-privado-list.md";

    /// <summary>
    /// Nomes de tipos de colecao mutavel que devem ser privados.
    /// </summary>
    private static readonly HashSet<string> MutableCollectionTypeNames = new(StringComparer.Ordinal)
    {
        "List",
        "IList",
        "ICollection",
        "Collection",
        "HashSet",
        "ISet"
    };

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Verificar fields publicos/internos de colecao mutavel
        foreach (var member in type.GetMembers())
        {
            if (member is IFieldSymbol field)
            {
                // Ignorar estaticos
                if (field.IsStatic)
                    continue;

                // Ignorar gerados pelo compilador (backing fields)
                if (field.IsImplicitlyDeclared)
                    continue;

                // Verificar se e colecao mutavel e nao e privada
                if (IsMutableCollectionType(field.Type) &&
                    field.DeclaredAccessibility != Accessibility.Private)
                {
                    return new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = context.ProjectName,
                        File = context.RelativeFilePath,
                        Line = GetFieldLineNumber(field, context.LineNumber),
                        Message = $"O field '{field.Name}' da classe '{type.Name}' e uma " +
                                  $"colecao mutavel ({GetCollectionTypeName(field.Type)}) com " +
                                  $"acessibilidade '{field.DeclaredAccessibility}'. " +
                                  $"Colecoes de entidades filhas devem ser declaradas como " +
                                  $"'private List<T>' para garantir encapsulamento",
                        LlmHint = $"Alterar o field '{field.Name}' para 'private'. " +
                                  $"Expor via propriedade IReadOnlyList<T> com AsReadOnly(). " +
                                  $"Consultar ADR DE-036 para exemplos"
                    };
                }
            }
            else if (member is IPropertySymbol property)
            {
                // Ignorar estaticos
                if (property.IsStatic)
                    continue;

                // Ignorar herdados
                if (!SymbolEqualityComparer.Default.Equals(property.ContainingType, type))
                    continue;

                // Ignorar overrides
                if (property.IsOverride)
                    continue;

                // Verificar propriedades publicas de colecao mutavel
                if (IsMutableCollectionType(property.Type) &&
                    property.DeclaredAccessibility == Accessibility.Public)
                {
                    return new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = context.ProjectName,
                        File = context.RelativeFilePath,
                        Line = GetPropertyLineNumber(property, context.LineNumber),
                        Message = $"A propriedade publica '{property.Name}' da classe " +
                                  $"'{type.Name}' expoe uma colecao mutavel " +
                                  $"({GetCollectionTypeName(property.Type)}). " +
                                  $"Colecoes devem ser expostas como IReadOnlyList<T> " +
                                  $"via AsReadOnly(), nunca como List<T> ou ICollection<T>",
                        LlmHint = $"Trocar o tipo da propriedade '{property.Name}' para " +
                                  $"IReadOnlyList<T> e retornar _field.AsReadOnly(). " +
                                  $"Consultar ADR DE-036 e DE-037 para exemplos"
                    };
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o tipo e uma colecao mutavel (List, IList, ICollection, etc.).
    /// </summary>
    private static bool IsMutableCollectionType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
            return false;

        if (!namedType.IsGenericType)
            return false;

        var typeName = namedType.Name;
        return MutableCollectionTypeNames.Contains(typeName);
    }

    /// <summary>
    /// Obtem o nome do tipo de colecao para a mensagem de erro.
    /// </summary>
    private static string GetCollectionTypeName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var typeArg = namedType.TypeArguments.Length > 0
                ? namedType.TypeArguments[0].Name
                : "T";
            return $"{namedType.Name}<{typeArg}>";
        }

        return typeSymbol.Name;
    }

    /// <summary>
    /// Obtem o numero da linha onde o field e declarado.
    /// </summary>
    private static int GetFieldLineNumber(IFieldSymbol field, int fallbackLineNumber)
    {
        var location = field.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
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
