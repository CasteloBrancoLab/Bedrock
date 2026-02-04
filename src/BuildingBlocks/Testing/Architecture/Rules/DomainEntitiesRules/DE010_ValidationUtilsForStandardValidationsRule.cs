using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-010: Métodos Validate* devem usar ValidationUtils para validações padrão.
/// ValidationUtils fornece métodos padronizados (ValidateIsRequired, ValidateMinLength, ValidateMaxLength)
/// que garantem consistência, mensagens padronizadas e manutenção centralizada.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Métodos <c>Validate*</c> (públicos e estáticos) devem conter pelo menos uma chamada a <c>ValidationUtils</c></item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Método <c>IsValid</c> (orquestra chamadas a Validate*, não valida diretamente)</item>
///   <item>Método <c>IsValidInternal</c> (override de EntityBase)</item>
///   <item>Métodos <c>Validate*Internal</c> (helpers privados de validação por operação)</item>
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE010_ValidationUtilsForStandardValidationsRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE010_ValidationUtilsForStandardValidations";

    public override string Description =>
        "Métodos Validate* devem usar ValidationUtils para validações padrão (DE-010)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-010-validationutils-para-validacoes-padrao.md";

    /// <summary>
    /// Nome da classe utilitária de validação.
    /// </summary>
    private const string ValidationUtilsClassName = "ValidationUtils";

    /// <summary>
    /// Sufixo de métodos *Internal que são helpers privados de validação por operação.
    /// </summary>
    private const string InternalSuffix = "Internal";

    /// <summary>
    /// Nome do método de instância protegido que é exceção (override de EntityBase).
    /// </summary>
    private const string IsValidInternalMethodName = "IsValidInternal";

    /// <summary>
    /// Nome do método IsValid que orquestra chamadas a Validate* (não valida diretamente).
    /// </summary>
    private const string IsValidMethodName = "IsValid";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Ignorar métodos sem corpo (abstratos, extern)
            if (method.IsAbstract || method.IsExtern)
                continue;

            // Ignorar métodos herdados de object (ToString, Equals, GetHashCode)
            if (IsObjectMethod(method))
                continue;

            // Ignorar property accessors, operators, etc.
            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Ignorar IsValidInternal (override protegido de EntityBase)
            if (method.Name == IsValidInternalMethodName)
                continue;

            // Ignorar IsValid (orquestra chamadas a Validate*, não valida diretamente)
            if (method.Name == IsValidMethodName)
                continue;

            // Verificar apenas métodos Validate* (não IsValid que é orquestrador)
            if (!method.Name.StartsWith("Validate", StringComparison.Ordinal))
                continue;

            // Ignorar métodos Validate*Internal (helpers privados de validação por operação)
            if (method.Name.EndsWith(InternalSuffix, StringComparison.Ordinal))
                continue;

            // Ignorar métodos que não são public static (já cobertos pela DE-009)
            if (!method.IsStatic || method.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Método Validate* público e estático encontrado - verificar se usa ValidationUtils
            if (!ContainsValidationUtilsCall(method))
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"Método '{method.Name}' da classe '{type.Name}' não usa ValidationUtils para validações padrão. " +
                              $"Métodos Validate* devem delegar para ValidationUtils.ValidateIsRequired, " +
                              $"ValidateMinLength ou ValidateMaxLength",
                    LlmHint = $"Refatorar o método '{method.Name}' da classe '{type.Name}' para usar " +
                              $"ValidationUtils.ValidateIsRequired, ValidationUtils.ValidateMinLength e/ou " +
                              $"ValidationUtils.ValidateMaxLength ao invés de implementar validação inline. " +
                              $"Consultar ADR DE-010 para exemplos de uso correto"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o corpo do método contém pelo menos uma chamada a ValidationUtils.*.
    /// Busca invocações do tipo <c>ValidationUtils.ValidateIsRequired(...)</c>,
    /// <c>ValidationUtils.ValidateMinLength(...)</c>, etc.
    /// </summary>
    private static bool ContainsValidationUtilsCall(IMethodSymbol method)
    {
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                if (descendant is InvocationExpressionSyntax invocation &&
                    IsValidationUtilsInvocation(invocation))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Verifica se uma invocação é uma chamada a ValidationUtils.* (member access).
    /// </summary>
    private static bool IsValidationUtilsInvocation(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        // Verificar se o receiver é "ValidationUtils"
        if (memberAccess.Expression is IdentifierNameSyntax identifier)
            return identifier.Identifier.Text == ValidationUtilsClassName;

        return false;
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
