using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.CodeStyleRules;

/// <summary>
/// Regra CS-004a: Todos os metodos publicos de uma factory devem retornar o mesmo tipo.
/// Factories que retornam tipos diferentes violam SRP.
/// </summary>
public sealed class CS004a_FactorySingleReturnTypeRule : CodeStyleRuleBase
{
    public override string Name => "CS004a_FactorySingleReturnType";

    public override string Description =>
        "Todos os metodos publicos de uma factory devem retornar o mesmo tipo (CS-004)";

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

        if (publicMethods.Count < 2)
            return null;

        var distinctReturnTypes = publicMethods
            .Select(static m => m.ReturnType.ToDisplayString())
            .Distinct()
            .ToList();

        if (distinctReturnTypes.Count <= 1)
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
            Message = $"A factory '{type.Name}' retorna tipos diferentes: " +
                      $"{string.Join(", ", distinctReturnTypes)}. " +
                      $"Cada factory deve retornar apenas um tipo (SRP)",
            LlmHint = $"Dividir '{type.Name}' em factories separadas, uma por tipo de retorno. " +
                      $"Naming: '{{ReturnType}}Factory'"
        };
    }
}
