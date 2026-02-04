using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-009: Métodos Validate* e IsValid estáticos devem ser públicos e estáticos.
/// Validação deve ser acessível de qualquer camada (controllers, serviços, consumers)
/// para permitir fail-fast e single source of truth.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Métodos cujo nome começa com <c>Validate</c> devem ser <c>public static</c></item>
///   <item>Método <c>IsValid</c> estático deve ser <c>public static</c></item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Método de instância <c>IsValidInternal</c> (override de EntityBase, é <c>protected</c>)</item>
///   <item>Métodos <c>Validate*Internal</c> (helpers privados de validação por operação)</item>
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE009_ValidateMethodsPublicStaticRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE009_ValidateMethodsPublicStatic";

    public override string Description =>
        "Métodos Validate* e IsValid devem ser públicos e estáticos para validação antecipada em camadas externas (DE-009)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-009-metodos-validate-publicos-e-estaticos.md";

    /// <summary>
    /// Nome do método de instância protegido que é exceção (override de EntityBase).
    /// </summary>
    private const string IsValidInternalMethodName = "IsValidInternal";

    /// <summary>
    /// Sufixo de métodos *Internal que são helpers privados de validação por operação
    /// (ex: ValidateReferencedAggregateRootForRegisterNewInternal).
    /// Esses métodos são privados por design e não fazem parte da API pública de validação.
    /// </summary>
    private const string InternalSuffix = "Internal";

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

            // Verificar apenas métodos de validação (Validate* e IsValid)
            if (!IsValidationMethod(method))
                continue;

            // Ignorar métodos Validate*Internal (helpers privados de validação por operação)
            // Ex: ValidateReferencedAggregateRootForRegisterNewInternal
            if (method.Name.EndsWith(InternalSuffix, StringComparison.Ordinal))
                continue;

            // Método de validação encontrado - verificar se é public e static
            if (!method.IsStatic || method.DeclaredAccessibility != Accessibility.Public)
            {
                var issues = new List<string>();

                if (!method.IsStatic)
                    issues.Add("estático");

                if (method.DeclaredAccessibility != Accessibility.Public)
                    issues.Add("público");

                var issueDescription = string.Join(" e ", issues);

                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"Método '{method.Name}' da classe '{type.Name}' deve ser {issueDescription}. " +
                              $"Métodos Validate* e IsValid devem ser públicos e estáticos para permitir " +
                              $"validação antecipada em camadas externas",
                    LlmHint = $"Alterar o método '{method.Name}' da classe '{type.Name}' para 'public static'. " +
                              $"Métodos de validação devem ser acessíveis de qualquer camada (controllers, serviços, consumers) " +
                              $"para permitir fail-fast e single source of truth conforme ADR DE-009"
                };
            }
        }

        return null;
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
