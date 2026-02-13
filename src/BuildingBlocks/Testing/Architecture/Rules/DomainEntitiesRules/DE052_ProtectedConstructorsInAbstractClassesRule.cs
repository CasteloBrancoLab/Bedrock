using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-052: Construtores em classes abstratas de dominio devem ser protegidos.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Todos os construtores em classes abstratas que herdam de EntityBase
///         devem ter acessibilidade <c>protected</c></item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes concretas (verificadas por DE-019/DE-020)</item>
///   <item>Classes que nao herdam de EntityBase</item>
///   <item>Construtores estaticos (implicitos)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE052_ProtectedConstructorsInAbstractClassesRule : DomainEntitiesGeneralRuleBase
{
    // Properties
    public override string Name => "DE052_ProtectedConstructorsInAbstractClasses";

    public override string Description =>
        "Construtores em classes abstratas devem ser protegidos para permitir heranca (DE-052)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-052-construtores-protegidos-em-classes-abstratas.md";

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

            if (method.MethodKind != MethodKind.Constructor)
                continue;

            if (method.IsImplicitlyDeclared)
                continue;

            // Construtores devem ser protected em classes abstratas
            if (method.DeclaredAccessibility != Accessibility.Protected)
            {
                var paramDesc = method.Parameters.Length == 0
                    ? "vazio"
                    : $"com {method.Parameters.Length} parametro(s)";

                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = GetMethodLineNumber(method, context.LineNumber),
                    Message = $"O construtor {paramDesc} da classe abstrata '{type.Name}' " +
                              $"tem acessibilidade '{method.DeclaredAccessibility}' mas deveria " +
                              $"ser 'protected'. Construtores em classes abstratas devem ser " +
                              $"protegidos para que classes filhas possam chamar base()",
                    LlmHint = $"Alterar a acessibilidade do construtor para 'protected'. " +
                              $"Classes filhas precisam de construtores protegidos na pai " +
                              $"para chamar base() (vazio) e base(...) (completo). " +
                              $"Consultar ADR DE-052 para exemplos"
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
