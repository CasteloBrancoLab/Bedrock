using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-054: Hierarquias de heranca em entidades de dominio nao devem exceder
/// 2 niveis alem de EntityBase (abstrata -> concreta).
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>A profundidade de heranca de classes concretas que herdam de EntityBase
///         nao deve exceder EntityBase -> Abstrata -> Concreta</item>
///   <item>Nao deve haver mais de uma classe abstrata intermediaria na cadeia</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes que nao herdam de EntityBase</item>
///   <item>Classes abstratas, estaticas, records</item>
///   <item>EntityBase na propria contagem (e infraestrutura)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE054_MaxInheritanceDepthRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE054_MaxInheritanceDepth";

    public override string Description =>
        "Hierarquias de heranca em entidades nao devem exceder 2 niveis alem de EntityBase (DE-054)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-054-heranca-vs-composicao-em-entidades.md";

    /// <summary>
    /// Numero maximo de classes abstratas intermediarias entre EntityBase e a classe concreta.
    /// O padrao e: EntityBase -> [1 abstrata] -> concreta (maximo 1 abstrata intermediaria).
    /// </summary>
    private const int MaxAbstractIntermediaries = 1;

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Contar niveis de classes abstratas entre EntityBase e o tipo concreto
        var abstractCount = 0;
        var current = type.BaseType;

        while (current is not null)
        {
            var baseName = current.Name;

            // Parar ao chegar em EntityBase
            if (baseName == EntityBaseTypeName ||
                (current.IsGenericType && current.ConstructedFrom.Name == EntityBaseTypeName))
                break;

            if (current.IsAbstract && current.TypeKind == TypeKind.Class)
                abstractCount++;

            current = current.BaseType;
        }

        if (abstractCount > MaxAbstractIntermediaries)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A classe '{type.Name}' tem {abstractCount} classe(s) " +
                          $"abstrata(s) intermediaria(s) na hierarquia de heranca " +
                          $"(maximo permitido: {MaxAbstractIntermediaries}). " +
                          $"Hierarquias profundas sao frageis. " +
                          $"Considere usar composicao em vez de heranca",
                LlmHint = $"Refatorar a hierarquia de heranca para usar no maximo " +
                          $"EntityBase -> 1 abstrata -> concreta. " +
                          $"Se precisar de mais niveis, considere composicao. " +
                          $"Consultar ADR DE-054 para exemplos de heranca vs composicao"
            };
        }

        return null;
    }
}
