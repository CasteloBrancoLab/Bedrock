using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.OutboxRules;

/// <summary>
/// OB-006b: Repositorios outbox de BC devem ser sealed — sao a camada final
/// da arquitectura em tres camadas (Interface → Base → BC sealed).
/// </summary>
public sealed class OB006b_OutboxRepositorySealedRule : OutboxRuleBase
{
    public override string Name => "OB006b_OutboxRepositorySealed";

    public override string Description =>
        "Repositorios outbox de BC devem ser sealed (OB-006).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/outbox/OB-006-repositorio-tres-camadas.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        if (IsBuildingBlockProject(context.ProjectName))
            return null;

        if (!InheritsFromBaseClass(type, OutboxRepositoryBaseName))
            return null;

        if (type.IsSealed)
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Classe '{type.Name}' herda de OutboxPostgreSqlRepositoryBase mas nao e sealed. " +
                      $"Repositorios outbox de BC sao a camada final e devem ser sealed.",
            LlmHint = $"Adicionar modificador 'sealed' na declaracao de '{type.Name}'. Consulte a ADR OB-006."
        };
    }
}
