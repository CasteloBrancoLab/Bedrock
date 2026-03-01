using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.OutboxRules;

/// <summary>
/// OB-006a: Classes que implementam IOutboxRepository (em projetos BC) devem herdar
/// de OutboxPostgreSqlRepositoryBase — nao implementar IOutboxRepository diretamente.
/// </summary>
public sealed class OB006a_OutboxRepositoryInheritsBaseRule : OutboxRuleBase
{
    public override string Name => "OB006a_OutboxRepositoryInheritsBase";

    public override string Description =>
        "Repositorios outbox de BC devem herdar de OutboxPostgreSqlRepositoryBase (OB-006).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/outbox/OB-006-repositorio-tres-camadas.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        if (IsBuildingBlockProject(context.ProjectName))
            return null;

        if (!ImplementsInterfaceTransitively(type, OutboxRepositoryInterfaceName))
            return null;

        if (InheritsFromBaseClass(type, OutboxRepositoryBaseName))
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Classe '{type.Name}' implementa IOutboxRepository mas nao herda de " +
                      $"OutboxPostgreSqlRepositoryBase. Repositorios outbox devem seguir a " +
                      $"arquitectura em tres camadas.",
            LlmHint = $"Alterar '{type.Name}' para herdar de OutboxPostgreSqlRepositoryBase " +
                      $"e implementar ConfigureInternal com options.WithTableName(\"{{bc}}_outbox\"). " +
                      $"Consulte a ADR OB-006."
        };
    }
}
