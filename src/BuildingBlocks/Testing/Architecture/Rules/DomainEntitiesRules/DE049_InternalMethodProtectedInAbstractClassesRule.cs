using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-049: Metodos *Internal em classes abstratas devem ser protegidos.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Metodos que terminam com <c>Internal</c> em classes abstratas que herdam de EntityBase
///         devem ter acessibilidade <c>protected</c></item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes concretas (sealed/nao-abstratas) — verificadas por outras regras</item>
///   <item>Classes que nao herdam de EntityBase</item>
///   <item>Metodos estaticos, metodos implicitos</item>
///   <item>Metodos herdados de EntityBase (ex: <c>RegisterNewInternal</c>)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE049_InternalMethodProtectedInAbstractClassesRule : DomainEntitiesGeneralRuleBase
{
    // Properties
    public override string Name => "DE049_InternalMethodProtectedInAbstractClasses";

    public override string Description =>
        "Metodos *Internal em classes abstratas devem ser protegidos para permitir acesso das classes filhas (DE-049)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-049-metodos-internal-protegidos-em-classes-abstratas.md";

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

            // Apenas metodos que terminam com Internal
            if (!method.Name.EndsWith("Internal", StringComparison.Ordinal))
                continue;

            // Ignorar metodos herdados (declarados em outra classe)
            if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, type))
                continue;

            // *Internal em classes abstratas deve ser protected
            // Aceitar tambem protected abstract (para metodos como IsValidConcreteInternal)
            if (method.DeclaredAccessibility != Accessibility.Protected &&
                method.DeclaredAccessibility != Accessibility.ProtectedOrInternal)
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
                              $"ser 'protected'. Metodos *Internal em classes abstratas devem " +
                              $"ser protegidos para que classes filhas possam compor " +
                              $"suas operacoes de negocio usando esses blocos",
                    LlmHint = $"Alterar a acessibilidade do metodo '{method.Name}' para " +
                              $"'protected'. Em classes abstratas, *Internal e o unico " +
                              $"mecanismo de acesso ao estado da classe pai pelas filhas. " +
                              $"Consultar ADR DE-049 para exemplos"
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
