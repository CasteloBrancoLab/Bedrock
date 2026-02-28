using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-005: MessageBase, CommandBase, EventBase e QueryBase devem ser abstract records.
/// </summary>
public sealed class MS005_BaseTypesAbstractRecordRule : MessageGeneralRuleBase
{
    public override string Name => "MS005_BaseTypesAbstractRecord";

    public override string Description =>
        "MessageBase, CommandBase, EventBase e QueryBase devem ser abstract records (MS-005)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-005-abstract-record-hierarquia-mensagens.md";

    /// <summary>
    /// Nomes dos tipos base que devem ser abstract records.
    /// </summary>
    private static readonly HashSet<string> BaseTypeNames = new(StringComparer.Ordinal)
    {
        "MessageBase", "CommandBase", "EventBase", "QueryBase"
    };

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        if (!BaseTypeNames.Contains(type.Name))
            return null;

        // Verificar: deve ser abstract
        if (!type.IsAbstract)
        {
            return CreateViolation(context,
                $"O tipo base '{type.Name}' deve ser abstract",
                $"Adicionar modificador 'abstract' na declaracao de '{type.Name}'");
        }

        // Verificar: deve ser record
        if (!type.IsRecord)
        {
            return CreateViolation(context,
                $"O tipo base '{type.Name}' deve ser record",
                $"Alterar '{type.Name}' para 'abstract record'");
        }

        return null;
    }

    private Violation CreateViolation(TypeContext context, string message, string llmHint)
    {
        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = context.ProjectName,
            File = context.RelativeFilePath,
            Line = context.LineNumber,
            Message = message,
            LlmHint = llmHint
        };
    }
}
