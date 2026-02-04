using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-018: <c>CreateFromExistingInfo</c> NÃO deve chamar métodos <c>Validate*</c>.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item><c>CreateFromExistingInfo</c> não deve invocar métodos cujo nome comece com <c>Validate</c></item>
///   <item><c>CreateFromExistingInfo</c> não deve invocar <c>IsValid</c></item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Entidades que não possuem <c>CreateFromExistingInfo</c> (verificado por DE-017)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE018_ReconstitutionDoesNotValidateRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE018_ReconstitutionDoesNotValidate";

    public override string Description =>
        "CreateFromExistingInfo NÃO deve chamar métodos Validate* nem IsValid (DE-018)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-018-reconstitution-nao-valida-dados.md";

    /// <summary>
    /// Nome do factory method de reconstitution.
    /// </summary>
    private const string CreateFromExistingInfoMethodName = "CreateFromExistingInfo";

    /// <summary>
    /// Prefixo de métodos de validação.
    /// </summary>
    private const string ValidatePrefix = "Validate";

    /// <summary>
    /// Nome do método orchestrator de validação.
    /// </summary>
    private const string IsValidMethodName = "IsValid";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Procurar método estático público CreateFromExistingInfo
        var createFromExistingInfoMethod = FindCreateFromExistingInfoMethod(type);

        // Se não existe, outra regra (DE-017) trata disso
        if (createFromExistingInfoMethod is null)
            return null;

        // Verificar se o corpo do método chama algum Validate* ou IsValid
        var validationCallName = FindValidationCallInMethodBody(createFromExistingInfoMethod);

        if (validationCallName is not null)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = GetMethodLineNumber(createFromExistingInfoMethod, context.LineNumber),
                Message = $"O método 'CreateFromExistingInfo' da classe '{type.Name}' chama " +
                          $"'{validationCallName}'. Reconstitution NÃO deve validar dados, " +
                          $"pois regras podem ter mudado desde que os dados foram persistidos",
                LlmHint = $"Remover a chamada a '{validationCallName}' do método 'CreateFromExistingInfo' " +
                          $"da classe '{type.Name}'. CreateFromExistingInfo deve apenas instanciar " +
                          $"via construtor completo SEM validação. " +
                          $"Consultar ADR DE-018 para fundamentação"
            };
        }

        return null;
    }

    /// <summary>
    /// Procura o método estático público CreateFromExistingInfo no tipo.
    /// </summary>
    private static IMethodSymbol? FindCreateFromExistingInfoMethod(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.Name == CreateFromExistingInfoMethodName &&
                method.IsStatic &&
                method.DeclaredAccessibility == Accessibility.Public &&
                method.MethodKind == MethodKind.Ordinary)
            {
                return method;
            }
        }

        return null;
    }

    /// <summary>
    /// Procura invocações de métodos Validate* ou IsValid no corpo do método.
    /// Usa a syntax tree para encontrar <c>InvocationExpressionSyntax</c>.
    /// </summary>
    /// <returns>Nome do método de validação encontrado, ou null se nenhum.</returns>
    private static string? FindValidationCallInMethodBody(IMethodSymbol method)
    {
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

                if (methodName.StartsWith(ValidatePrefix, StringComparison.Ordinal) ||
                    methodName == IsValidMethodName)
                {
                    return methodName;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Extrai o nome do método invocado a partir de uma <c>InvocationExpressionSyntax</c>.
    /// Suporta chamadas simples (<c>Validate()</c>) e member access (<c>instance.Validate()</c>).
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
