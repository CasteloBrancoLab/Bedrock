using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-039: Construtores que recebem colecoes devem fazer copia defensiva,
/// nunca guardar a referencia direta. Usar <c>[.. param]</c> ou <c>.ToList()</c>.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Construtores que recebem parametros de colecao (<c>IEnumerable&lt;T&gt;</c>,
///         <c>List&lt;T&gt;</c>, etc.) NAO devem atribuir diretamente ao field</item>
///   <item>A atribuicao deve usar spread (<c>[.. param]</c>), <c>.ToList()</c>
///         ou outra forma de copia</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Construtores sem parametros de colecao</item>
///   <item>Construtores gerados pelo compilador</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE039_DefensiveCopyCollectionInConstructorRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE039_DefensiveCopyCollectionInConstructor";

    public override string Description =>
        "Construtores devem fazer copia defensiva de colecoes, nunca guardar referencia direta (DE-039)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-039-defensive-copy-colecoes-construtor.md";

    /// <summary>
    /// Nomes de tipos de colecao que indicam um parametro de colecao.
    /// </summary>
    private static readonly HashSet<string> CollectionInterfaceNames = new(StringComparer.Ordinal)
    {
        "IEnumerable",
        "ICollection",
        "IList",
        "IReadOnlyList",
        "IReadOnlyCollection",
        "List",
        "Collection"
    };

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Constructor)
                continue;

            // Ignorar construtores sem parametros
            if (method.Parameters.Length == 0)
                continue;

            // Ignorar gerados pelo compilador
            if (method.IsImplicitlyDeclared)
                continue;

            // Encontrar parametros de colecao
            foreach (var param in method.Parameters)
            {
                if (!IsCollectionParameter(param))
                    continue;

                // Verificar se o construtor faz atribuicao direta do parametro
                var directAssignment = FindDirectAssignment(method, param.Name);
                if (directAssignment is not null)
                {
                    return new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = context.ProjectName,
                        File = context.RelativeFilePath,
                        Line = GetMethodLineNumber(method, context.LineNumber),
                        Message = $"O construtor da classe '{type.Name}' atribui o parametro " +
                                  $"de colecao '{param.Name}' diretamente ao field " +
                                  $"'{directAssignment}'. Isso compartilha a referencia " +
                                  $"e permite modificacoes externas. Usar copia defensiva: " +
                                  $"[.. {param.Name}] ou {param.Name}.ToList()",
                        LlmHint = $"Substituir a atribuicao direta do parametro '{param.Name}' " +
                                  $"por copia defensiva: '_field = [.. {param.Name}]'. " +
                                  $"Consultar ADR DE-039 para exemplos"
                    };
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o parametro e de um tipo de colecao.
    /// </summary>
    private static bool IsCollectionParameter(IParameterSymbol param)
    {
        if (param.Type is not INamedTypeSymbol namedType)
            return false;

        if (!namedType.IsGenericType)
            return false;

        return CollectionInterfaceNames.Contains(namedType.Name);
    }

    /// <summary>
    /// Verifica se o construtor faz atribuicao direta de um parametro de colecao
    /// a um field (ex: <c>_field = param</c> sem copia).
    /// </summary>
    /// <returns>O nome do field que recebe a atribuicao direta, ou null.</returns>
    private static string? FindDirectAssignment(IMethodSymbol constructor, string parameterName)
    {
        foreach (var syntaxRef in constructor.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is not AssignmentExpressionSyntax assignment)
                    continue;

                // Verificar se o lado direito e simplesmente o nome do parametro
                // (atribuicao direta: _field = param)
                if (assignment.Right is not IdentifierNameSyntax rightIdentifier)
                    continue;

                if (rightIdentifier.Identifier.ValueText != parameterName)
                    continue;

                // Verificar se o lado esquerdo e um field (ou this.field)
                var fieldName = GetAssignmentTarget(assignment.Left);
                if (fieldName is not null)
                    return fieldName;
            }
        }

        return null;
    }

    /// <summary>
    /// Obtem o nome do alvo da atribuicao (field ou propriedade).
    /// </summary>
    private static string? GetAssignmentTarget(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess
                when memberAccess.Expression is ThisExpressionSyntax =>
                memberAccess.Name.Identifier.ValueText,
            _ => null
        };
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
