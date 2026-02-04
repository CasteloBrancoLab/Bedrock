using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-025: Métodos <c>*Internal</c> que retornam <c>bool</c> devem usar variável
/// intermediária <c>isSuccess</c> para armazenar o resultado combinado de Set* antes de retornar.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Métodos <c>*Internal</c> que retornam <c>bool</c> devem declarar variável <c>isSuccess</c></item>
///   <item>A variável facilita debug, breakpoints e análise estática</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Métodos que não terminam com <c>Internal</c></item>
///   <item>Métodos que não retornam <c>bool</c></item>
///   <item>Métodos que chamam apenas um único Set* (retorno direto é aceitável)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE025_IntermediateVariablesInValidationRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE025_IntermediateVariablesInValidation";

    public override string Description =>
        "Métodos *Internal com múltiplos Set* devem usar variável intermediária isSuccess (DE-025)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-025-variaveis-intermediarias-para-legibilidade-e-debug.md";

    /// <summary>
    /// Sufixo dos métodos internos.
    /// </summary>
    private const string InternalSuffix = "Internal";

    /// <summary>
    /// Prefixo dos métodos de atribuição.
    /// </summary>
    private const string SetPrefix = "Set";

    /// <summary>
    /// Nome da variável intermediária esperada.
    /// </summary>
    private const string IsSuccessVariableName = "isSuccess";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Apenas métodos *Internal
            if (!method.Name.EndsWith(InternalSuffix, StringComparison.Ordinal))
                continue;

            // Apenas métodos que retornam bool
            if (method.ReturnType.SpecialType != SpecialType.System_Boolean)
                continue;

            // Ignorar abstratos/extern
            if (method.IsAbstract || method.IsExtern)
                continue;

            // Contar chamadas a Set* no corpo
            var setCallCount = CountSetCallsInBody(method);

            // Se chama múltiplos Set*, deve ter variável isSuccess
            if (setCallCount >= 2 && !HasIsSuccessVariable(method))
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O método '{method.Name}' da classe '{type.Name}' chama {setCallCount} " +
                              $"métodos Set* mas não declara variável '{IsSuccessVariableName}'. " +
                              $"Variáveis intermediárias facilitam debug, breakpoints e análise estática",
                    LlmHint = $"Refatorar '{method.Name}' para armazenar resultado combinado em " +
                              $"'bool {IsSuccessVariableName} = SetX(...) & SetY(...);' e " +
                              $"'return {IsSuccessVariableName};'. " +
                              $"Consultar ADR DE-025 para exemplos de uso correto"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Conta quantas vezes métodos Set* são chamados no corpo do método.
    /// </summary>
    private static int CountSetCallsInBody(IMethodSymbol method)
    {
        var count = 0;

        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is not InvocationExpressionSyntax invocation)
                    continue;

                var methodName = GetInvokedMethodName(invocation);

                if (methodName is not null &&
                    methodName.StartsWith(SetPrefix, StringComparison.Ordinal))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Verifica se o método declara uma variável chamada <c>isSuccess</c>.
    /// </summary>
    private static bool HasIsSuccessVariable(IMethodSymbol method)
    {
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is VariableDeclaratorSyntax variableDeclarator &&
                    variableDeclarator.Identifier.ValueText == IsSuccessVariableName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Extrai o nome do método invocado.
    /// </summary>
    private static string? GetInvokedMethodName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            _ => null
        };
    }

    /// <summary>
    /// Obtém o número da linha onde o método é declarado.
    /// </summary>
    private static int GetMethodLineNumber(IMethodSymbol method, int fallbackLineNumber)
    {
        var location = method.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
