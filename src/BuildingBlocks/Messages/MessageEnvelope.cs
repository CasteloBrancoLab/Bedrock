namespace Bedrock.BuildingBlocks.Messages;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: MessageEnvelope - Tipo Concreto Para Deserialização do Envelope
═══════════════════════════════════════════════════════════════════════════════

MessageEnvelope é o tipo concreto que o consumer usa no primeiro estágio
de deserialização. Diferente de MessageBase (abstract), qualquer serializer
(JSON, Protobuf, Avro, etc.) consegue deserializar para MessageEnvelope.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Desacoplado do Formato de Serialização
───────────────────────────────────────────────────────────────────────────────

O consumer NÃO deve depender de recursos específicos de um serializer
(ex: ler nós JSON, extrair campos Protobuf). MessageEnvelope é um record
concreto que qualquer serializer consegue hidratar.

✅ serializer.Deserialize<MessageEnvelope>(raw) // funciona com qualquer formato
❌ JsonDocument.Parse(raw).GetProperty("Metadata") // acoplado a JSON

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Fluxo de Deserialização em Dois Estágios
───────────────────────────────────────────────────────────────────────────────

1. Deserializa para MessageEnvelope (concreto) → obtém Metadata + SchemaName
2. Usa Metadata.SchemaName para resolver o tipo concreto
3. Deserializa novamente para o tipo concreto (ex: UserRegisteredEvent)
4. Handler recebe o tipo concreto (que herda de MessageBase)

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Somente Metadata — Sem Payload
───────────────────────────────────────────────────────────────────────────────

MessageEnvelope carrega APENAS MessageMetadata. O payload não é
deserializado neste estágio — evita custo desnecessário quando o consumer
só precisa rotear a mensagem.

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Concrete record for first-stage deserialization of the message envelope.
/// Serializer-agnostic — works with JSON, Protobuf, Avro, or any other format.
/// </summary>
public sealed record MessageEnvelope(MessageMetadata Metadata);
