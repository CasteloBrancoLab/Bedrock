namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Classe base para regras Messages que analisam TODOS os tipos
/// (nao apenas mensagens concretas). Usada por regras que verificam
/// tipos base (MessageBase, CommandBase, etc.) e MessageMetadata.
/// </summary>
public abstract class MessageGeneralRuleBase : Rule
{
    public override string Category => "Messages";
}
