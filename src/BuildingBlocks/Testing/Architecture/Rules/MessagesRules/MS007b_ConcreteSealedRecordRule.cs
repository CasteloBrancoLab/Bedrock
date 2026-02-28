namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-007b: Mensagens concretas devem ser sealed record.
/// </summary>
public sealed class MS007b_ConcreteSealedRecordRule : MessageRuleBase
{
    public override string Name => "MS007b_ConcreteSealedRecord";
    public override string Description => "Mensagens concretas devem ser sealed record (MS-007)";
    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-007-concretos-herdam-base-tipada.md";

    protected override Violation? AnalyzeMessageType(TypeContext context)
    {
        var type = context.Type;

        // MessageRuleBase ja filtra IsRecord e !IsAbstract.
        // Verificar apenas se e sealed.
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
            Message = $"A mensagem '{type.Name}' deve ser 'sealed record'",
            LlmHint = $"Adicionar modificador 'sealed' na declaracao de '{type.Name}'"
        };
    }
}
