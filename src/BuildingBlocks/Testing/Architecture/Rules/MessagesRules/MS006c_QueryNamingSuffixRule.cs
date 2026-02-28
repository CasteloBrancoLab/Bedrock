namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-006c: Queries devem terminar com sufixo "Query".
/// </summary>
public sealed class MS006c_QueryNamingSuffixRule : MessageRuleBase
{
    public override string Name => "MS006c_QueryNamingSuffix";
    public override string Description => "Queries devem terminar com sufixo 'Query' (MS-006)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-006-nomenclatura-commands-events-queries.md";

    protected override Violation? AnalyzeMessageType(TypeContext context)
    {
        var type = context.Type;
        var kind = GetMessageKind(type);

        if (kind != "Query")
            return null;

        if (type.Name.EndsWith("Query", StringComparison.Ordinal))
            return null;

        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = $"A query '{type.Name}' deve terminar com sufixo 'Query' (ex: '{type.Name}Query')",
            LlmHint = $"Renomear '{type.Name}' para '{type.Name}Query' ou outro nome com sufixo 'Query'"
        };
    }
}
