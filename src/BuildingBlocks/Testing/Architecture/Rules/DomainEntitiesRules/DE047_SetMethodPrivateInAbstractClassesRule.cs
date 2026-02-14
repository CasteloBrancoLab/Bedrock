using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-047: Metodos Set* em classes abstratas devem ser privados, nao protegidos.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Metodos <c>Set*</c> em classes abstratas que herdam de EntityBase
///         devem ter acessibilidade <c>private</c></item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes concretas (sealed/nao-abstratas) — verificadas por outras regras</item>
///   <item>Classes que nao herdam de EntityBase</item>
///   <item>Metodos estaticos, metodos implicitos</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE047_SetMethodPrivateInAbstractClassesRule : DomainEntitiesGeneralRuleBase
{
    // Properties
    public override string Name => "DE047_SetMethodPrivateInAbstractClasses";

    public override string Description =>
        "Metodos Set* em classes abstratas devem ser privados para proteger invariantes (DE-047)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-047-metodos-set-privados-em-classes-abstratas.md";

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

            if (method.IsStatic || method.IsImplicitlyDeclared)
                continue;

            // Apenas metodos Set*
            if (!method.Name.StartsWith("Set", StringComparison.Ordinal))
                continue;

            // Set* deve ser privado em classes abstratas
            if (method.DeclaredAccessibility != Accessibility.Private)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O metodo '{method.Name}' da classe abstrata '{type.Name}' " +
                              $"tem acessibilidade '{method.DeclaredAccessibility}' mas deveria " +
                              $"ser 'private'. Metodos Set* em classes abstratas devem ser " +
                              $"privados para proteger invariantes. Classes filhas devem " +
                              $"usar metodos *Internal protegidos para alterar estado",
                    LlmHint = $"Alterar a acessibilidade do metodo '{method.Name}' para " +
                              $"'private'. Se classes filhas precisam alterar esse estado, " +
                              $"criar um metodo *Internal protegido que chame o Set* privado. " +
                              $"Consultar ADR DE-047 para exemplos"
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
