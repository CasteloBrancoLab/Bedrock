using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-035: Construtores de entidades de dominio NAO devem conter logica
/// de validacao. Validacao deve ocorrer em <c>RegisterNew</c> (dados novos)
/// enquanto <c>CreateFromExistingInfo</c> reconstitui sem validar.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Construtores NAO devem chamar metodos <c>Validate*</c></item>
///   <item>Construtores NAO devem lancar excecoes de validacao de negocio
///         (throw com ArgumentException, ValidationException, etc.)</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estaticas, records, enums, interfaces, structs</item>
///   <item>Construtores sem parametros (nao tem o que validar)</item>
///   <item>Construtores gerados pelo compilador</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE035_ConstructorDoesNotValidateRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE035_ConstructorDoesNotValidate";

    public override string Description =>
        "Construtores nao devem conter logica de validacao - usar RegisterNew para validar (DE-035)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-035-antipadrao-construtor-que-valida.md";

    /// <summary>
    /// Prefixo dos metodos de validacao.
    /// </summary>
    private const string ValidatePrefix = "Validate";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Constructor)
                continue;

            // Ignorar construtor sem parametros
            if (method.Parameters.Length == 0)
                continue;

            // Ignorar construtores gerados pelo compilador
            if (method.IsImplicitlyDeclared)
                continue;

            // Verificar se o construtor chama Validate*
            var validateCall = FindValidateCallInConstructor(method);
            if (validateCall is not null)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O construtor da classe '{type.Name}' chama '{validateCall}'. " +
                              $"Construtores nao devem conter logica de validacao. " +
                              $"Validacao deve ocorrer em RegisterNew (dados novos). " +
                              $"O construtor e usado por CreateFromExistingInfo e Clone, " +
                              $"que reconstituem dados ja validados",
                    LlmHint = $"Remover chamada a '{validateCall}' do construtor de '{type.Name}'. " +
                              $"Mover validacao para o handler de RegisterNewInternal. " +
                              $"Consultar ADR DE-035 para exemplos"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Procura chamadas a metodos Validate* no corpo do construtor.
    /// </summary>
    /// <returns>O nome do metodo Validate encontrado, ou null.</returns>
    private static string? FindValidateCallInConstructor(IMethodSymbol constructor)
    {
        foreach (var syntaxRef in constructor.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is not InvocationExpressionSyntax invocation)
                    continue;

                var methodName = GetInvokedMethodName(invocation);

                if (methodName is not null &&
                    methodName.StartsWith(ValidatePrefix, StringComparison.Ordinal))
                {
                    return methodName;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Extrai o nome do metodo invocado.
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
    /// Extrai o nome base de uma expressao.
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
