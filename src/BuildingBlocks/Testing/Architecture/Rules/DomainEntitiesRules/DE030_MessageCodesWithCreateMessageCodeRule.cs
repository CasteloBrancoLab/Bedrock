using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-030: Métodos <c>Validate*</c> devem usar <c>CreateMessageCode&lt;T&gt;</c>
/// para gerar códigos de mensagem, garantindo formato consistente
/// <c>{EntityName}.{PropertyName}</c>.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Métodos Validate* que chamam ValidationUtils devem passar o propertyName
///         via <c>CreateMessageCode&lt;T&gt;</c></item>
///   <item>Verifica que o método <c>CreateMessageCode</c> é invocado dentro de métodos Validate*</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Métodos que não são Validate* (não se aplica)</item>
///   <item>Métodos Validate* que não chamam ValidationUtils (podem ter validação customizada)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE030_MessageCodesWithCreateMessageCodeRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE030_MessageCodesWithCreateMessageCode";

    public override string Description =>
        "Métodos Validate* devem usar CreateMessageCode<T> para códigos de mensagem (DE-030)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-030-message-codes-com-createmessagecode.md";

    /// <summary>
    /// Prefixo dos métodos de validação.
    /// </summary>
    private const string ValidatePrefix = "Validate";

    /// <summary>
    /// Nome do tipo ValidationUtils.
    /// </summary>
    private const string ValidationUtilsName = "ValidationUtils";

    /// <summary>
    /// Nome do método para criação de message codes.
    /// </summary>
    private const string CreateMessageCodeName = "CreateMessageCode";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Apenas métodos Validate*
            if (!method.Name.StartsWith(ValidatePrefix, StringComparison.Ordinal))
                continue;

            // Ignorar métodos abstratos ou extern
            if (method.IsAbstract || method.IsExtern)
                continue;

            // Ignorar gerados pelo compilador
            if (method.IsImplicitlyDeclared)
                continue;

            // Se chama ValidationUtils mas NÃO chama CreateMessageCode, é violação
            if (CallsValidationUtils(method) && !CallsCreateMessageCode(method))
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
                              $"ValidationUtils mas não usa CreateMessageCode<T> para gerar " +
                              $"códigos de mensagem. Usar CreateMessageCode<{type.Name}>" +
                              $"(propertyName) para garantir formato consistente " +
                              $"'{{EntityName}}.{{PropertyName}}'",
                    LlmHint = $"Substituir strings hardcoded por " +
                              $"'CreateMessageCode<{type.Name}>(Metadata.PropertyName)' " +
                              $"em '{method.Name}'. Consultar ADR DE-030 para exemplos"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o método chama algum método de ValidationUtils.
    /// </summary>
    private static bool CallsValidationUtils(IMethodSymbol method)
    {
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is not InvocationExpressionSyntax invocation)
                    continue;

                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression is IdentifierNameSyntax identifier &&
                    identifier.Identifier.ValueText == ValidationUtilsName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Verifica se o método chama CreateMessageCode.
    /// </summary>
    private static bool CallsCreateMessageCode(IMethodSymbol method)
    {
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is not InvocationExpressionSyntax invocation)
                    continue;

                var methodName = GetInvokedMethodName(invocation);

                if (methodName is not null &&
                    methodName == CreateMessageCodeName)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Extrai o nome do método invocado, incluindo genéricos.
    /// </summary>
    private static string? GetInvokedMethodName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            GenericNameSyntax genericName => genericName.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => GetNameFromExpression(memberAccess.Name),
            _ => null
        };
    }

    /// <summary>
    /// Extrai o nome base de uma expressão, suportando genéricos.
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
