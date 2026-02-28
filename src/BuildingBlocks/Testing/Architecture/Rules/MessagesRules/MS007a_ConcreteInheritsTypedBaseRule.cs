using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-007a: Mensagens concretas nao devem herdar diretamente de MessageBase.
/// Devem usar CommandBase, EventBase ou QueryBase.
/// </summary>
public sealed class MS007a_ConcreteInheritsTypedBaseRule : MessageRuleBase
{
    public override string Name => "MS007a_ConcreteInheritsTypedBase";

    public override string Description =>
        "Mensagens concretas devem herdar de CommandBase, EventBase ou QueryBase, nao de MessageBase diretamente (MS-007)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-007-concretos-herdam-base-tipada.md";

    protected override Violation? AnalyzeMessageType(TypeContext context)
    {
        var type = context.Type;

        // Se o tipo base direto for MessageBase, e uma violacao
        if (type.BaseType?.Name == MessageBaseTypeName)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A mensagem '{type.Name}' herda diretamente de MessageBase. Deve herdar de CommandBase, EventBase ou QueryBase",
                LlmHint = $"Alterar a heranca de '{type.Name}' de 'MessageBase' para 'CommandBase', 'EventBase' ou 'QueryBase'"
            };
        }

        return null;
    }
}
