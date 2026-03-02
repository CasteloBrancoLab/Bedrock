using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-011: Nenhum tipo deve declarar membros 'event' (delegates do C#).
/// Eventos de integracao sao records via Outbox, nao delegates .NET.
/// </summary>
public sealed class MS011_NoDotNetEventsRule : MessageGeneralRuleBase
{
    public override string Name => "MS011_NoDotNetEvents";

    public override string Description =>
        "Tipos nao devem declarar membros 'event' (delegate). Eventos sao records via Outbox (MS-011)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-011-eventos-integracao-via-outbox.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        foreach (var member in type.GetMembers())
        {
            if (member is IEventSymbol eventSymbol)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = context.LineNumber,
                    Message = $"O tipo '{type.Name}' declara o evento '{eventSymbol.Name}' " +
                              $"(delegate do C#). Eventos de integracao devem ser sealed records " +
                              $"que herdam de EventBase e sao publicados via Outbox",
                    LlmHint = $"Remover declaracao 'event {eventSymbol.Type.Name} {eventSymbol.Name}' " +
                              $"de '{type.Name}'. Usar sealed record : EventBase(...) via Outbox"
                };
            }
        }

        return null;
    }
}
