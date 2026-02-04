using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-044: Colecoes de entidades filhas NAO devem ter metodo <c>Set*</c>.
/// Toda modificacao deve ser via metodos de negocio especificos (Add, Remove,
/// Change*, etc.), nunca por substituicao em massa.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Entidades com colecoes de entidades filhas NAO devem ter metodos
///         cujo nome comece com <c>Set</c> e contenha o nome do tipo da
///         colecao (ex: <c>SetCompositeChildEntities</c>)</item>
///   <item>Metodos <c>Set*</c> que recebem colecoes (<c>IEnumerable&lt;T&gt;</c>,
///         <c>List&lt;T&gt;</c>, etc.) como parametro sao proibidos</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Metodos Set* para propriedades escalares (ex: SetFirstName, SetTitle)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE044_NoSetMethodForCollectionsRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE044_NoSetMethodForCollections";

    public override string Description =>
        "Colecoes de entidades filhas nao devem ter metodo Set* - usar metodos de negocio (DE-044)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-044-antipadrao-colecoes-sem-metodo-set.md";

    /// <summary>
    /// Prefixo de metodos Set.
    /// </summary>
    private const string SetPrefix = "Set";

    /// <summary>
    /// Nomes de tipos de colecao que indicam substituicao em massa.
    /// </summary>
    private static readonly HashSet<string> CollectionTypeNames = new(StringComparer.Ordinal)
    {
        "IEnumerable",
        "ICollection",
        "IList",
        "IReadOnlyList",
        "IReadOnlyCollection",
        "List",
        "Collection",
        "HashSet"
    };

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Apenas metodos Set*
            if (!method.Name.StartsWith(SetPrefix, StringComparison.Ordinal))
                continue;

            // Verificar se algum parametro e uma colecao
            foreach (var param in method.Parameters)
            {
                if (IsCollectionType(param.Type))
                {
                    return new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = context.ProjectName,
                        File = context.RelativeFilePath,
                        Line = GetMethodLineNumber(method, context.LineNumber),
                        Message = $"O metodo '{method.Name}' da classe '{type.Name}' " +
                                  $"e um metodo Set* que recebe uma colecao como parametro " +
                                  $"({param.Type.Name}<{GetTypeArgName(param.Type)}>). " +
                                  $"Colecoes de entidades filhas nao devem ter metodo Set. " +
                                  $"Usar metodos de negocio especificos (Add*, Remove*, " +
                                  $"Change*) para cada operacao",
                        LlmHint = $"Remover o metodo '{method.Name}' e substituir por " +
                                  $"metodos de negocio granulares. " +
                                  $"Consultar ADR DE-044 para exemplos"
                    };
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o tipo e uma colecao generica.
    /// </summary>
    private static bool IsCollectionType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
            return false;

        if (!namedType.IsGenericType)
            return false;

        return CollectionTypeNames.Contains(namedType.Name);
    }

    /// <summary>
    /// Obtem o nome do tipo argumento generico para a mensagem de erro.
    /// </summary>
    private static string GetTypeArgName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            namedType.TypeArguments.Length > 0)
        {
            return namedType.TypeArguments[0].Name;
        }

        return "T";
    }

    /// <summary>
    /// Obtem o numero da linha onde o metodo e declarado.
    /// </summary>
    private static int GetMethodLineNumber(IMethodSymbol method, int fallbackLineNumber)
    {
        var location = method.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
