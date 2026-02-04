using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-006: Métodos *Internal em entidades devem usar operador bitwise AND (&amp;)
/// ao invés de logical AND (&amp;&amp;) para combinar validações.
/// O operador &amp; garante que TODAS as validações executam, permitindo coletar
/// todos os erros de uma vez (Notification Pattern).
/// Exceções: classes abstratas, estáticas, records, enums, interfaces, structs.
/// </summary>
public sealed class DE006_BitwiseAndForValidationRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE006_BitwiseAndForValidation";
    public override string Description => "Métodos *Internal devem usar operador & ao invés de && para validação completa (DE-006)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/domain-entities/DE-006-operador-bitwise-and-para-validacao-completa.md";

    /// <summary>
    /// Sufixo dos métodos que devem usar operador &amp;.
    /// </summary>
    private const string InternalMethodSuffix = "Internal";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Analisar cada método *Internal procurando uso de &&
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Verificar apenas métodos cujo nome termina com "Internal"
            if (!method.Name.EndsWith(InternalMethodSuffix, StringComparison.Ordinal))
                continue;

            // Ignorar métodos sem corpo (abstratos, extern, partial sem implementação)
            if (method.IsAbstract || method.IsExtern)
                continue;

            // Inspecionar o corpo do método procurando &&
            var logicalAndLocation = FindLogicalAndInMethodBody(method);
            if (logicalAndLocation is not null)
            {
                var lineNumber = logicalAndLocation.GetLineSpan().StartLinePosition.Line + 1;

                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = lineNumber,
                    Message = $"Método '{method.Name}' da classe '{type.Name}' usa operador '&&' (logical AND). " +
                              $"Métodos *Internal devem usar '&' (bitwise AND) para garantir que todas as validações executem",
                    LlmHint = $"Substituir operador '&&' por '&' no método '{method.Name}' da classe '{type.Name}' " +
                              $"para garantir que todas as validações executem e todos os erros sejam coletados no ExecutionContext"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Procura uso do operador logical AND (&&) no corpo de um método.
    /// Usa a syntax tree do Roslyn para inspecionar expressões binárias.
    /// </summary>
    /// <returns>Location do primeiro && encontrado, ou null se não houver.</returns>
    private static Location? FindLogicalAndInMethodBody(IMethodSymbol method)
    {
        // Obter declaração do método na syntax tree
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            // Procurar todos os nós BinaryExpression com kind LogicalAndExpression (&&)
            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is BinaryExpressionSyntax binaryExpr &&
                    binaryExpr.IsKind(SyntaxKind.LogicalAndExpression))
                {
                    return binaryExpr.OperatorToken.GetLocation();
                }
            }
        }

        return null;
    }
}
