using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-048: Metodos Validate* em classes abstratas devem ser publicos e estaticos.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Metodos <c>Validate*</c> em classes abstratas que herdam de EntityBase
///         devem ser <c>public</c> e <c>static</c></item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes concretas (sealed/nao-abstratas) — verificadas por outras regras</item>
///   <item>Classes que nao herdam de EntityBase</item>
///   <item>Metodos implicitos</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE048_ValidateMethodPublicInAbstractClassesRule : Rule
{
    // Properties
    public override string Name => "DE048_ValidateMethodPublicInAbstractClasses";

    public override string Description =>
        "Metodos Validate* em classes abstratas devem ser publicos e estaticos (DE-048)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-048-metodos-validate-publicos-em-classes-abstratas.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // So se aplica a classes abstratas
        if (!type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        // So se aplica a classes que herdam de EntityBase
        if (!DomainEntityRuleBase.InheritsFromEntityBase(type))
            return null;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            if (method.IsImplicitlyDeclared)
                continue;

            // Apenas metodos Validate*
            if (!method.Name.StartsWith("Validate", StringComparison.Ordinal))
                continue;

            // Validate* deve ser public e static
            if (method.DeclaredAccessibility != Accessibility.Public || !method.IsStatic)
            {
                var issue = !method.IsStatic && method.DeclaredAccessibility != Accessibility.Public
                    ? "nao e publico nem estatico"
                    : !method.IsStatic
                        ? "nao e estatico"
                        : "nao e publico";

                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O metodo '{method.Name}' da classe abstrata '{type.Name}' " +
                              $"{issue}. Metodos Validate* em classes abstratas devem ser " +
                              $"publicos e estaticos, permitindo validacao antecipada por " +
                              $"camadas externas (controllers, servicos de aplicacao)",
                    LlmHint = $"Alterar o metodo '{method.Name}' para " +
                              $"'public static'. Metodos Validate* sao puros (sem side-effects) " +
                              $"e devem ser acessiveis de qualquer lugar. " +
                              $"Consultar ADR DE-048 para exemplos"
                };
            }
        }

        return null;
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
