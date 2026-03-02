using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.OutboxRules;

/// <summary>
/// OB-011b: Writers outbox de BC devem implementar uma marker interface propria
/// do BC (ex: IAuthOutboxWriter) que estenda IOutboxWriter&lt;MessageBase&gt;.
/// </summary>
public sealed class OB011b_OutboxWriterMarkerInterfaceRule : OutboxRuleBase
{
    public override string Name => "OB011b_OutboxWriterMarkerInterface";

    public override string Description =>
        "Writers outbox de BC devem implementar marker interface propria que estenda IOutboxWriter (OB-011).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/outbox/OB-011-marker-interfaces-outbox-por-bc.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        if (IsBuildingBlockProject(context.ProjectName))
            return null;

        if (!ImplementsInterfaceTransitively(type, OutboxWriterInterfaceName))
            return null;

        // Verificar se implementa pelo menos uma marker interface que estende IOutboxWriter
        var hasMarker = type.AllInterfaces.Any(iface =>
            iface.Name != OutboxWriterInterfaceName &&
            ImplementsInterfaceTransitively(iface, OutboxWriterInterfaceName) &&
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
            Message = $"Classe '{type.Name}' implementa IOutboxWriter mas nao implementa uma marker " +
                      $"interface propria do BC (ex: IAuthOutboxWriter).",
            LlmHint = $"Criar uma interface vazia (ex: I{{Bc}}OutboxWriter : IOutboxWriter<MessageBase>) " +
                      $"em 'Infra.CrossCutting.Messages/Outbox/Interfaces/' e implementa-la em '{type.Name}'. " +
                      $"Consulte a ADR OB-011."
        };
    }
}
