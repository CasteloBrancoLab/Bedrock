using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

/// <summary>
/// Regra CS-004c: Factories devem estar no namespace correto conforme o tipo de retorno.
/// - Factories em *.Factories.*.Events devem retornar tipos de *.Events
/// - Factories em *.Factories.*.Models devem retornar tipos de *.Models
/// </summary>
public sealed class CS004c_FactoryNamespaceRule : CodeStyleRuleBase
{
    public override string Name => "CS004c_FactoryNamespace";

    public override string Description =>
        "Factories em Events/ devem retornar tipos de *.Events, em Models/ tipos de *.Models (CS-004)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/code-style/CS-004-factory-srp-organizacao-namespace.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (!IsFactory(type))
            return null;

        var ns = type.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        // Determinar a subcategoria do namespace da factory
        string? expectedReturnCategory = null;

        if (ns.EndsWith(".Events", StringComparison.Ordinal) ||
            ns.Contains(".Events.", StringComparison.Ordinal))
            expectedReturnCategory = ".Events";
        else if (ns.EndsWith(".Models", StringComparison.Ordinal) ||
                 ns.Contains(".Models.", StringComparison.Ordinal))
            expectedReturnCategory = ".Models";
        else
            return null; // Factory na raiz (ex: AuthMessageMetadataFactory) — sem restricao de subcategoria

        var publicMethods = type.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(static m => m.DeclaredAccessibility == Accessibility.Public &&
                               m.MethodKind == MethodKind.Ordinary &&
                               !m.IsImplicitlyDeclared)
            .ToList();

        if (publicMethods.Count == 0)
            return null;

        var returnType = publicMethods[0].ReturnType;

        // Unwrap Task<T>
        if (returnType is INamedTypeSymbol namedReturn &&
            namedReturn.IsGenericType &&
            namedReturn.Name == "Task" &&
            namedReturn.TypeArguments.Length == 1)
        {
            returnType = namedReturn.TypeArguments[0];
        }

        var returnNs = returnType.ContainingNamespace?.ToDisplayString() ?? string.Empty;

        if (returnNs.Contains(expectedReturnCategory, StringComparison.Ordinal))
            return null;

        var location = type.Locations.FirstOrDefault(static l => l.IsInSource);
        var line = location?.GetLineSpan().StartLinePosition.Line + 1 ?? 0;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = line,
            Message = $"A factory '{type.Name}' esta em namespace '{ns}' " +
                      $"mas retorna '{returnType.Name}' de namespace '{returnNs}'. " +
                      $"Factories em *{expectedReturnCategory} devem retornar tipos de *{expectedReturnCategory}",
            LlmHint = $"Mover '{type.Name}' para o namespace correto conforme o tipo de retorno, " +
                      $"ou mover para um namespace raiz se o retorno nao se encaixa em Events/Models"
        };
    }
}
