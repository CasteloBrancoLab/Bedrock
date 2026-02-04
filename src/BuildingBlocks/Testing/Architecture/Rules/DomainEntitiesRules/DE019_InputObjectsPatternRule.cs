using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-019: Factory methods e métodos públicos de mutação devem receber
/// Input Objects (<c>readonly record struct</c>) ao invés de parâmetros primitivos individuais.
/// <para>
/// O que é verificado:
/// <list type="bullet">
///   <item><c>RegisterNew</c> deve ter parâmetro do tipo <c>readonly record struct</c></item>
///   <item><c>CreateFromExistingInfo</c> deve ter parâmetro do tipo <c>readonly record struct</c></item>
/// </list>
/// </para>
/// <para>
/// Exceções (não verificados por esta regra):
/// <list type="bullet">
///   <item>Classes abstratas, estáticas, records, enums, interfaces, structs</item>
///   <item>Entidades que não possuem os métodos (verificado por outras regras)</item>
///   <item>Parâmetro <c>ExecutionContext</c> (não é um Input Object)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE019_InputObjectsPatternRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE019_InputObjectsPattern";

    public override string Description =>
        "Factory methods devem receber Input Objects (readonly record struct), não parâmetros primitivos (DE-019)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-019-input-objects-pattern.md";

    /// <summary>
    /// Nomes dos factory methods que devem receber Input Objects.
    /// </summary>
    private static readonly string[] FactoryMethodNames =
    [
        "RegisterNew",
        "CreateFromExistingInfo"
    ];

    /// <summary>
    /// Nome do tipo de contexto de execução (não é Input Object).
    /// </summary>
    private const string ExecutionContextTypeName = "ExecutionContext";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Verificar apenas factory methods conhecidos
            if (!IsTargetFactoryMethod(method))
                continue;

            // Verificar se os parâmetros (exceto ExecutionContext) são readonly record struct
            var violation = CheckParametersAreInputObjects(method, type, context);
            if (violation is not null)
                return violation;
        }

        return null;
    }

    /// <summary>
    /// Verifica se o método é um dos factory methods alvo (público e estático).
    /// </summary>
    private static bool IsTargetFactoryMethod(IMethodSymbol method)
    {
        if (!method.IsStatic || method.DeclaredAccessibility != Accessibility.Public)
            return false;

        if (method.MethodKind != MethodKind.Ordinary)
            return false;

        foreach (var name in FactoryMethodNames)
        {
            if (method.Name == name)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Verifica se todos os parâmetros do método (exceto ExecutionContext) são readonly record structs.
    /// </summary>
    private Violation? CheckParametersAreInputObjects(
        IMethodSymbol method, INamedTypeSymbol type, TypeContext context)
    {
        foreach (var parameter in method.Parameters)
        {
            // Ignorar ExecutionContext
            if (parameter.Type.Name == ExecutionContextTypeName)
                continue;

            // Verificar se o tipo do parâmetro é readonly record struct
            if (!IsReadOnlyRecordStruct(parameter.Type))
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O parâmetro '{parameter.Name}' do método '{method.Name}' da classe " +
                              $"'{type.Name}' é do tipo '{parameter.Type.ToDisplayString()}', " +
                              $"que não é um 'readonly record struct'. " +
                              $"Factory methods devem receber Input Objects (readonly record struct)",
                    LlmHint = $"Criar um 'readonly record struct' como Input Object para o método " +
                              $"'{method.Name}' da classe '{type.Name}'. " +
                              $"Exemplo: 'public readonly record struct {method.Name}Input(...)'. " +
                              $"Consultar ADR DE-019 para exemplos de uso correto"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o tipo é um readonly record struct.
    /// </summary>
    private static bool IsReadOnlyRecordStruct(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
            return false;

        // Deve ser struct (ValueType)
        if (!namedType.IsValueType)
            return false;

        // Deve ser record
        if (!namedType.IsRecord)
            return false;

        // Deve ser readonly
        if (!namedType.IsReadOnly)
            return false;

        return true;
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
