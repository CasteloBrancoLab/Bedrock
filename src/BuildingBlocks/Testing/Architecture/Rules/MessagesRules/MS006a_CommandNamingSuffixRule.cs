namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-006a: Commands devem terminar com sufixo "Command".
/// </summary>
public sealed class MS006a_CommandNamingSuffixRule : MessageRuleBase
{
    public override string Name => "MS006a_CommandNamingSuffix";
    public override string Description => "Commands devem terminar com sufixo 'Command' (MS-006)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-006-nomenclatura-commands-events-queries.md";

    protected override Violation? AnalyzeMessageType(TypeContext context)
    {
        var type = context.Type;
        var kind = GetMessageKind(type);

        if (kind != "Command")
            return null;

        if (type.Name.EndsWith("Command", StringComparison.Ordinal))
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"O command '{type.Name}' deve terminar com sufixo 'Command' (ex: '{type.Name}Command')",
            LlmHint = $"Renomear '{type.Name}' para '{type.Name}Command' ou outro nome com sufixo 'Command'"
        };
    }
}
