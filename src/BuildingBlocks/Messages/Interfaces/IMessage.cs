namespace Bedrock.BuildingBlocks.Messages.Interfaces;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: IMessage - Contrato Padrão Para Todas as Mensagens
═══════════════════════════════════════════════════════════════════════════════

Toda mensagem (Command, Event, Query) que cruza fronteiras de processo
carrega um envelope padronizado (MessageMetadata) para rastreabilidade,
roteamento e multi-tenancy.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Metadata Como Objeto — Não Flat
───────────────────────────────────────────────────────────────────────────────

O envelope é encapsulado em MessageMetadata (record separado) para:
- Deserialização em dois estágios (metadata primeiro, payload depois)
- Construtor limpo nos tipos concretos (1 parâmetro vs 7)
- Extensibilidade (adicionar campo ao envelope não quebra concretos)

✅ IMessage { MessageMetadata Metadata { get; } }
❌ IMessage { Guid MessageId { get; } DateTimeOffset Timestamp { get; } ... }

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: SchemaName Dentro do Metadata
───────────────────────────────────────────────────────────────────────────────

SchemaName é campo do MessageMetadata (não da IMessage diretamente).
Isso permite que o consumer deserialize APENAS o MessageMetadata e já
tenha SchemaName para resolver o tipo concreto e direcionar ao handler.

Fluxo do consumer:
1. Deserializa MessageMetadata do JSON bruto
2. Lê Metadata.SchemaName → resolve tipo concreto
3. Deserializa payload completo para o tipo concreto

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Defines the standard envelope contract for all messages (commands, events, queries)
/// in this bounded context.
/// </summary>
public interface IMessage
{
    /// <summary>Standard envelope with routing, tracing, multi-tenancy metadata, and SchemaName.</summary>
    MessageMetadata Metadata { get; }
}
