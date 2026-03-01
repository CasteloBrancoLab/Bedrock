using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.OutboxRules;

/// <summary>
/// OB-008: Writers outbox de BC devem compor MessageOutboxWriter, nao herdar dele.
/// MessageOutboxWriter e sealed por design — a composicao e a abordagem correcta.
/// </summary>
public sealed class OB008_OutboxWriterCompositionRule : OutboxRuleBase
{
    public override string Name => "OB008_OutboxWriterComposition";

    public override string Description =>
        "Writers outbox de BC devem usar composicao sobre heranca com MessageOutboxWriter (OB-008).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/outbox/OB-008-composicao-sobre-heranca-outboxwriter.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        if (IsBuildingBlockProject(context.ProjectName))
            return null;

        if (!ImplementsInterfaceTransitively(type, OutboxWriterInterfaceName))
            return null;

        if (!InheritsFromBaseClass(type, MessageOutboxWriterName))
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"Classe '{type.Name}' herda de MessageOutboxWriter. Writers outbox de BC " +
                      $"devem usar composicao (conter um MessageOutboxWriter), nao heranca.",
            LlmHint = $"Alterar '{type.Name}' para compor MessageOutboxWriter como campo privado " +
                      $"e delegar EnqueueAsync. MessageOutboxWriter e sealed por design. " +
                      $"Consulte a ADR OB-008."
        };
    }
}
