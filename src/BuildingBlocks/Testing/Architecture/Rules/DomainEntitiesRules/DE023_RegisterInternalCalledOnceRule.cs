using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-023: Cada método público deve chamar <c>RegisterNewInternal</c> ou
/// <c>RegisterChangeInternal</c> no máximo <b>uma única vez</b>.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Métodos públicos que chamam Register*Internal devem chamá-lo apenas uma vez</item>
///   <item>Múltiplas chamadas causam múltiplos clones, versões e auditoria inconsistente</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Métodos que não chamam Register*Internal (não aplicável)</item>
///   <item>Métodos de validação, métodos herdados de Object</item>
///   <item><c>CreateFromExistingInfo</c> e <c>Clone</c> (não usam Register*Internal)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE023_RegisterInternalCalledOnceRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE023_RegisterInternalCalledOnce";

    public override string Description =>
        "Register*Internal deve ser chamado no máximo uma vez por método público (DE-023)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-023-register-internal-chamado-uma-unica-vez.md";

    /// <summary>
    /// Nomes dos métodos de registro interno cujas chamadas devem ser únicas.
    /// </summary>
    private const string RegisterNewInternalName = "RegisterNewInternal";
    private const string RegisterChangeInternalName = "RegisterChangeInternal";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Apenas métodos públicos
            if (method.DeclaredAccessibility != Accessibility.Public)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Ignorar métodos herdados de Object
            if (IsObjectMethod(method))
                continue;

            // Ignorar métodos de validação
            if (IsValidationMethod(method))
                continue;

            // Ignorar métodos abstratos ou extern
            if (method.IsAbstract || method.IsExtern)
                continue;

            // Contar chamadas a Register*Internal
            var callCount = CountRegisterInternalCalls(method);

            if (callCount > 1)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O método '{method.Name}' da classe '{type.Name}' chama " +
                              $"Register*Internal {callCount} vezes. Cada método público deve chamar " +
                              $"Register*Internal no máximo UMA vez para garantir versões consistentes, " +
                              $"um único clone e auditoria correta",
                    LlmHint = $"Refatorar o método '{method.Name}' da classe '{type.Name}' para fazer " +
                              $"UMA ÚNICA chamada a Register*Internal, combinando todos os métodos *Internal " +
                              $"dentro do handler usando operador '&'. " +
                              $"Consultar ADR DE-023 para exemplos de uso correto"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Conta quantas vezes o método chama RegisterNewInternal ou RegisterChangeInternal.
    /// Usa a syntax tree para encontrar invocações.
    /// </summary>
    private static int CountRegisterInternalCalls(IMethodSymbol method)
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

                if (methodName is null)
                    continue;

                if (methodName == RegisterNewInternalName ||
                    methodName == RegisterChangeInternalName)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Extrai o nome do método invocado, considerando genéricos como
    /// <c>RegisterChangeInternal&lt;T, TInput&gt;</c>.
    /// </summary>
    private static string? GetInvokedMethodName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => GetNameFromExpression(memberAccess.Name),
            GenericNameSyntax genericName => genericName.Identifier.ValueText,
            _ => null
        };
    }

    /// <summary>
    /// Extrai o nome base de uma expressão, suportando nomes genéricos.
    /// </summary>
    private static string? GetNameFromExpression(SimpleNameSyntax name)
    {
        return name switch
        {
            GenericNameSyntax genericName => genericName.Identifier.ValueText,
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            _ => null
        };
    }

    /// <summary>
    /// Obtém o número da linha onde o método é declarado, ou fallback para a linha da classe.
    /// </summary>
    private static int GetMethodLineNumber(IMethodSymbol method, int fallbackLineNumber)
    {
        var location = method.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
