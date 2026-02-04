using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-021: Métodos públicos de mutação (Change*) devem delegar lógica para
/// métodos *Internal correspondentes, não implementar lógica diretamente.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Para cada método público Change*, deve existir um método *Internal correspondente</item>
///   <item>Exemplo: <c>ChangeName</c> deve ter <c>ChangeNameInternal</c></item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item><c>RegisterNew</c>, <c>CreateFromExistingInfo</c>, <c>Clone</c> (factory methods e lifecycle)</item>
///   <item>Métodos de validação (<c>Validate*</c>, <c>IsValid</c>)</item>
///   <item>Métodos herdados de Object (ToString, Equals, etc.)</item>
///   <item>Métodos que não começam com <c>Change</c></item>
/// </list>
/// </para>
/// </summary>
public sealed class DE021_PublicMethodsDelegateToInternalRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE021_PublicMethodsDelegateToInternal";

    public override string Description =>
        "Métodos públicos Change* devem ter método *Internal correspondente para reutilização (DE-021)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-021-metodos-publicos-vs-metodos-internos.md";

    /// <summary>
    /// Prefixo de métodos públicos de mutação.
    /// </summary>
    private const string ChangePrefix = "Change";

    /// <summary>
    /// Sufixo dos métodos internos.
    /// </summary>
    private const string InternalSuffix = "Internal";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Coletar nomes de todos os métodos *Internal existentes
        var internalMethodNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in type.GetMembers())
        {
            if (member is IMethodSymbol method &&
                method.Name.EndsWith(InternalSuffix, StringComparison.Ordinal))
            {
                internalMethodNames.Add(method.Name);
            }
        }

        // Verificar cada método público Change*
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Apenas métodos públicos de instância (não estáticos)
            if (method.DeclaredAccessibility != Accessibility.Public)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Apenas métodos que começam com "Change"
            if (!method.Name.StartsWith(ChangePrefix, StringComparison.Ordinal))
                continue;

            // Ignorar métodos herdados de Object
            if (IsObjectMethod(method))
                continue;

            // Verificar se existe método *Internal correspondente
            var expectedInternalName = method.Name + InternalSuffix;
            if (!internalMethodNames.Contains(expectedInternalName))
            {
                // Também verificar se o corpo do método chama algum *Internal
                // (pode ser que o *Internal tenha um nome diferente, ex: ChangeCompositeChildEntityTitle
                //  delega para ChangeCompositeChildEntityTitleInternal)
                if (!CallsAnyInternalMethod(method))
                {
                    return new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = context.ProjectName,
                        File = context.RelativeFilePath,
                        Line = GetMethodLineNumber(method, context.LineNumber),
                        Message = $"O método público '{method.Name}' da classe '{type.Name}' não possui " +
                                  $"método '{expectedInternalName}' correspondente. " +
                                  $"Métodos públicos Change* devem delegar lógica para métodos *Internal",
                        LlmHint = $"Criar método 'private bool {expectedInternalName}(ExecutionContext executionContext, ...)' " +
                                  $"na classe '{type.Name}' que implementa a lógica de negócio. " +
                                  $"O método público '{method.Name}' deve chamar '{expectedInternalName}' " +
                                  $"dentro do handler de RegisterChangeInternal. " +
                                  $"Consultar ADR DE-021 para exemplos de uso correto"
                    };
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o corpo do método chama algum método que termina com "Internal".
    /// </summary>
    private static bool CallsAnyInternalMethod(IMethodSymbol method)
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
                    methodName.EndsWith(InternalSuffix, StringComparison.Ordinal))
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
