namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-006b: Events devem terminar com sufixo "Event".
/// </summary>
public sealed class MS006b_EventNamingSuffixRule : MessageRuleBase
{
    public override string Name => "MS006b_EventNamingSuffix";
    public override string Description => "Events devem terminar com sufixo 'Event' (MS-006)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-006-nomenclatura-commands-events-queries.md";

    protected override Violation? AnalyzeMessageType(TypeContext context)
    {
        var type = context.Type;
        var kind = GetMessageKind(type);

        if (kind != "Event")
            return null;

        if (type.Name.EndsWith("Event", StringComparison.Ordinal))
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"O event '{type.Name}' deve terminar com sufixo 'Event' (ex: '{type.Name}Event')",
            LlmHint = $"Renomear '{type.Name}' para '{type.Name}Event' ou outro nome com sufixo 'Event'"
        };
    }
}
