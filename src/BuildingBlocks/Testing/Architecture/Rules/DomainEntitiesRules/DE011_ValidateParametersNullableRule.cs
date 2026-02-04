using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-011: Parâmetros de valor em métodos Validate* devem ser nullable por design.
/// A obrigatoriedade é uma regra de negócio decidida em runtime (via metadata), não em compile-time.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item>Métodos <c>Validate*</c> (públicos e estáticos) devem ter parâmetros de valor nullable</item>
///   <item>Reference types devem usar <c>T?</c> (NullableAnnotation.Annotated)</item>
///   <item>Value types devem usar <c>Nullable&lt;T&gt;</c> (ex: <c>int?</c>, <c>BirthDate?</c>)</item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Primeiro parâmetro <c>ExecutionContext</c> (sempre obrigatório, não é dado de entrada)</item>
///   <item>Método <c>IsValid</c> (orquestra chamadas a Validate*, tem parâmetros próprios)</item>
///   <item>Método <c>IsValidInternal</c> (override de EntityBase)</item>
///   <item>Métodos <c>Validate*Internal</c> (helpers privados de validação por operação)</item>
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE011_ValidateParametersNullableRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE011_ValidateParametersNullable";

    public override string Description =>
        "Parâmetros de valor em métodos Validate* devem ser nullable por design (DE-011)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-011-parametros-validate-nullable-por-design.md";

    /// <summary>
    /// Nome do tipo ExecutionContext (primeiro parâmetro, sempre non-null).
    /// </summary>
    private const string ExecutionContextTypeName = "ExecutionContext";

    /// <summary>
    /// Sufixo de métodos *Internal que são helpers privados.
    /// </summary>
    private const string InternalSuffix = "Internal";

    /// <summary>
    /// Nome do método de instância protegido que é exceção (override de EntityBase).
    /// </summary>
    private const string IsValidInternalMethodName = "IsValidInternal";

    /// <summary>
    /// Nome do método IsValid que orquestra chamadas a Validate*.
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

            // Ignorar métodos herdados de object
            if (IsObjectMethod(method))
                continue;

            // Ignorar property accessors, operators, etc.
            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            // Ignorar IsValidInternal (override protegido de EntityBase)
            if (method.Name == IsValidInternalMethodName)
                continue;

            // Ignorar IsValid (orquestra chamadas a Validate*, tem parâmetros próprios)
            if (method.Name == IsValidMethodName)
                continue;

            // Verificar apenas métodos Validate*
            if (!method.Name.StartsWith("Validate", StringComparison.Ordinal))
                continue;

            // Ignorar métodos Validate*Internal (helpers privados)
            if (method.Name.EndsWith(InternalSuffix, StringComparison.Ordinal))
                continue;

            // Ignorar métodos que não são public static (já cobertos pela DE-009)
            if (!method.IsStatic || method.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Verificar cada parâmetro de valor (excluindo ExecutionContext)
            foreach (var parameter in method.Parameters)
            {
                // Ignorar o parâmetro ExecutionContext (sempre non-null)
                if (parameter.Type.Name == ExecutionContextTypeName)
                    continue;

                // Verificar se o parâmetro é nullable
                if (!IsParameterNullable(parameter))
                {
                    return new Violation
                    {
                        Rule = Name,
                        Severity = DefaultSeverity,
                        Adr = AdrPath,
                        Project = context.ProjectName,
                        File = context.RelativeFilePath,
                        Line = GetMethodLineNumber(method, context.LineNumber),
                        Message = $"Parâmetro '{parameter.Name}' do método '{method.Name}' da classe '{type.Name}' " +
                                  $"deve ser nullable ({parameter.Type.Name}?). " +
                                  $"A obrigatoriedade é decidida em runtime via metadata, não em compile-time",
                        LlmHint = $"Alterar o parâmetro '{parameter.Name}' do método '{method.Name}' da classe '{type.Name}' " +
                                  $"de '{parameter.Type.ToDisplayString()}' para '{parameter.Type.Name}?'. " +
                                  $"A obrigatoriedade deve ser validada em runtime via ValidationUtils.ValidateIsRequired " +
                                  $"e metadata (IsRequired), não forçada pelo compilador conforme ADR DE-011"
                    };
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se um parâmetro é nullable.
    /// Para reference types: <c>NullableAnnotation.Annotated</c> (ex: <c>string?</c>).
    /// Para value types: tipo deve ser <c>Nullable&lt;T&gt;</c> (ex: <c>int?</c>).
    /// </summary>
    private static bool IsParameterNullable(IParameterSymbol parameter)
    {
        var type = parameter.Type;

        // Value types: verificar se é Nullable<T>
        if (type.IsValueType)
        {
            return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        }

        // Reference types: verificar NullableAnnotation
        return parameter.NullableAnnotation == NullableAnnotation.Annotated;
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
