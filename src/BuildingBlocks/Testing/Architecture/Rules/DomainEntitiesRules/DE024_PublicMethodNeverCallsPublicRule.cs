using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-024: Métodos públicos NUNCA chamam outros métodos públicos da mesma classe.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>O corpo de um método público não deve invocar outro método público da mesma classe</item>
///   <item>Side-effects de métodos públicos devem ser isolados e previsíveis</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Métodos herdados de Object (ToString, Equals, etc.)</item>
///   <item>Métodos de validação (<c>Validate*</c>, <c>IsValid</c>) — são queries sem side-effects</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE024_PublicMethodNeverCallsPublicRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE024_PublicMethodNeverCallsPublic";

    public override string Description =>
        "Métodos públicos NUNCA chamam outros métodos públicos da mesma classe (DE-024)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-024-metodo-publico-nunca-chama-outro-publico.md";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Coletar nomes de todos os métodos públicos (exceto validação e Object)
        var publicMethodNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.DeclaredAccessibility != Accessibility.Public)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            if (IsObjectMethod(method))
                continue;

            // Não excluir Validate* da lista — queremos detectar se outro público
            // chama Validate* diretamente (isso é OK, pois são queries).
            // Mas excluímos na verificação do chamador abaixo.
            publicMethodNames.Add(method.Name);
        }

        // Verificar cada método público
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol callerMethod)
                continue;

            if (callerMethod.DeclaredAccessibility != Accessibility.Public)
                continue;

            if (callerMethod.MethodKind != MethodKind.Ordinary)
                continue;

            if (IsObjectMethod(callerMethod))
                continue;

            // Métodos de validação são queries sem side-effects — podem chamar outros públicos
            if (IsValidationMethod(callerMethod))
                continue;

            // Ignorar métodos abstratos ou extern
            if (callerMethod.IsAbstract || callerMethod.IsExtern)
                continue;

            // Verificar se o corpo chama outro método público da mesma classe
            var calledPublicMethod = FindPublicMethodCallInBody(callerMethod, publicMethodNames);
            if (calledPublicMethod is not null)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(callerMethod, context.LineNumber),
                    Message = $"O método público '{callerMethod.Name}' da classe '{type.Name}' chama " +
                              $"outro método público '{calledPublicMethod}'. Métodos públicos devem usar " +
                              $"construtores privados ou métodos *Internal para reutilização, " +
                              $"evitando acúmulo de side-effects",
                    LlmHint = $"Refatorar '{callerMethod.Name}' para não chamar '{calledPublicMethod}'. " +
                              $"Se precisam da mesma lógica, usar construtor privado ou método *Internal " +
                              $"compartilhado. Consultar ADR DE-024 para exemplos de uso correto"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Procura no corpo do método uma invocação de qualquer método público da mesma classe.
    /// Ignora chamadas a métodos de validação (Validate*, IsValid) pois são queries sem side-effects.
    /// </summary>
    /// <returns>Nome do método público chamado, ou null.</returns>
    private static string? FindPublicMethodCallInBody(
        IMethodSymbol callerMethod, HashSet<string> publicMethodNames)
    {
        foreach (var syntaxRef in callerMethod.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is not InvocationExpressionSyntax invocation)
                    continue;

                var methodName = GetInvokedMethodName(invocation);

                if (methodName is null)
                    continue;

                // Ignorar chamada ao próprio método (recursão — raro mas possível)
                if (methodName == callerMethod.Name)
                    continue;

                // Ignorar métodos de validação (são queries sem side-effects)
                if (methodName.StartsWith("Validate", StringComparison.Ordinal) ||
                    methodName == "IsValid")
                    continue;

                // Verificar se é um método público da mesma classe
                if (publicMethodNames.Contains(methodName))
                    return methodName;
            }
        }

        return null;
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
