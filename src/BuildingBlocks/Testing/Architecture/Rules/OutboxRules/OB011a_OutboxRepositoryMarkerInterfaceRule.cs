using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.OutboxRules;

/// <summary>
/// OB-011a: Repositorios outbox de BC devem implementar uma marker interface
/// propria do BC (ex: IAuthOutboxRepository) que estenda IOutboxRepository.
/// Nao devem depender diretamente de IOutboxRepository no DI.
/// </summary>
public sealed class OB011a_OutboxRepositoryMarkerInterfaceRule : OutboxRuleBase
{
    public override string Name => "OB011a_OutboxRepositoryMarkerInterface";

    public override string Description =>
        "Repositorios outbox de BC devem implementar marker interface propria que estenda IOutboxRepository (OB-011).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/outbox/OB-011-marker-interfaces-outbox-por-bc.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        if (IsBuildingBlockProject(context.ProjectName))
            return null;

        if (!InheritsFromBaseClass(type, OutboxRepositoryBaseName))
            return null;

        // Verificar se implementa pelo menos uma marker interface que estende IOutboxRepository
        var hasMarker = type.AllInterfaces.Any(iface =>
            iface.Name != OutboxRepositoryInterfaceName &&
            iface.Name != OutboxReaderInterfaceName &&
            ImplementsInterfaceTransitively(iface, OutboxRepositoryInterfaceName) &&
            IsMarkerInterface(iface));

        if (hasMarker)
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Classe '{type.Name}' nao implementa uma marker interface propria do BC " +
                      $"que estenda IOutboxRepository (ex: IAuthOutboxRepository).",
            LlmHint = $"Criar uma interface vazia (ex: I{{Bc}}OutboxRepository : IOutboxRepository) " +
                      $"no namespace '*.Outbox.Interfaces' e implementa-la em '{type.Name}'. " +
                      $"Consulte a ADR OB-011 e IN-006."
        };
    }
}
