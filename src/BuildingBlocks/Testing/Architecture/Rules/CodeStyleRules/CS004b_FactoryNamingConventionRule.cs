using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

/// <summary>
/// Regra CS-004b: O nome da factory deve ser '{ReturnType}Factory'.
/// Ex: UserRegisteredEventFactory retorna UserRegisteredEvent.
/// </summary>
public sealed class CS004b_FactoryNamingConventionRule : CodeStyleRuleBase
{
    public override string Name => "CS004b_FactoryNamingConvention";

    public override string Description =>
        "O nome da factory deve ser '{ReturnType}Factory' (CS-004)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/code-style/CS-004-factory-srp-organizacao-namespace.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (!IsFactory(type))
            return null;

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

        var expectedName = returnType.Name + "Factory";

        if (string.Equals(type.Name, expectedName, StringComparison.Ordinal))
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
            Message = $"A factory '{type.Name}' retorna '{returnType.Name}' " +
                      $"mas deveria chamar-se '{expectedName}'",
            LlmHint = $"Renomear '{type.Name}' para '{expectedName}'"
        };
    }
}
