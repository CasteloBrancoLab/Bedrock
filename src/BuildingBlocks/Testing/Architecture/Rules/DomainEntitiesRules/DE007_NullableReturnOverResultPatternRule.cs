using System.Collections.Frozen;
using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-007: Métodos de factory e modificação em entidades devem retornar <c>T?</c>
/// (nullable) ao invés de usar Result Pattern (<c>Result&lt;T&gt;</c>, <c>Either&lt;T&gt;</c>, etc.).
/// As mensagens de erro/warning/info são coletadas no <c>ExecutionContext</c>,
/// eliminando a necessidade de wrappers Result.
/// Exceções: classes abstratas, estáticas, records, enums, interfaces, structs.
/// Métodos herdados de object, propriedade accessors e operadores são ignorados.
/// </summary>
public sealed class DE007_NullableReturnOverResultPatternRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE007_NullableReturnOverResultPattern";

    public override string Description =>
        "Métodos de entidades devem retornar T? ao invés de Result<T> (DE-007)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-007-retorno-nullable-vs-result-pattern.md";

    /// <summary>
    /// Nomes de tipos genéricos que indicam uso de Result Pattern.
    /// Inclui variações comuns encontradas em bibliotecas .NET.
    /// </summary>
    private static readonly FrozenSet<string> ResultPatternTypeNames = FrozenSet.ToFrozenSet(
    [
        "Result",
        "OperationResult",
        "ServiceResult",
        "ActionResult",
        "CommandResult",
        "QueryResult",
        "Either",
        "OneOf",
        "ErrorOr",
        "Validation",
        "Outcome"
    ]);

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Analisar cada método público procurando retorno Result-like
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Ignorar: não-públicos, propriedade accessors, construtores, operators
            if (method.DeclaredAccessibility != Accessibility.Public ||
                method.MethodKind != MethodKind.Ordinary)
                continue;

            // Ignorar métodos herdados de object (ToString, Equals, GetHashCode, etc.)
            if (IsObjectMethod(method))
                continue;

            // Ignorar métodos de validação (Validate*, IsValid) - retornam bool por design
            if (IsValidationMethod(method))
                continue;

            // Verificar se o retorno é um tipo Result-like
            var resultTypeName = GetResultPatternTypeName(method.ReturnType);
            if (resultTypeName is not null)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = context.LineNumber,
                    Message = $"Método '{method.Name}' da classe '{type.Name}' retorna " +
                              $"'{method.ReturnType.ToDisplayString()}' (Result Pattern). " +
                              $"Deve retornar '{type.Name}?' (nullable) com mensagens no ExecutionContext",
                    LlmHint = $"Alterar o retorno do método '{method.Name}' da classe '{type.Name}' " +
                              $"de '{method.ReturnType.ToDisplayString()}' para '{type.Name}?' (nullable). " +
                              $"Coletar mensagens de erro/warning/info no ExecutionContext ao invés de usar Result Pattern"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se o tipo de retorno é um tipo Result-like genérico.
    /// Analisa o nome do tipo (sem considerar namespace) contra a lista de nomes conhecidos.
    /// </summary>
    /// <returns>Nome do tipo Result-like encontrado, ou null se não for Result Pattern.</returns>
    private static string? GetResultPatternTypeName(ITypeSymbol returnType)
    {
        // Verificar tipo direto
        if (ResultPatternTypeNames.Contains(returnType.Name))
            return returnType.Name;

        // Verificar tipo genérico (Result<T>, Either<L,R>, etc.)
        if (returnType is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var originalName = namedType.ConstructedFrom.Name;
            if (ResultPatternTypeNames.Contains(originalName))
                return originalName;
        }

        return null;
    }
}
