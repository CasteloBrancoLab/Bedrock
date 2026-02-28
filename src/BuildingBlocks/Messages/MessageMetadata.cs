namespace Bedrock.BuildingBlocks.Messages;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: MessageMetadata - Envelope Padrão Deserializável
═══════════════════════════════════════════════════════════════════════════════

MessageMetadata encapsula todos os campos de envelope comuns a qualquer
mensagem (Command, Event, Query). É um record separado para permitir
deserialização em dois estágios:

1. Deserializa MessageMetadata (roteamento, tracing, multi-tenancy, SchemaName)
2. Usa SchemaName para resolver o tipo concreto e deserializar o payload

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: SchemaName = Namespace Completo (Auto-Computado pelo Produtor)
───────────────────────────────────────────────────────────────────────────────

SchemaName é o full type name do tipo concreto. O produtor (MessageBase)
computa via GetType().FullName e injeta no Metadata na construção.
O consumer deserializa MessageMetadata e usa SchemaName para resolver o
tipo concreto e direcionar ao handler correto.

✅ "MyBoundedContext.Infra.CrossCutting.Messages.V1.Events.UserRegisteredEvent"
❌ "UserRegisteredEvent" // ambíguo entre versões e BCs

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Campos Derivados do ExecutionContext
───────────────────────────────────────────────────────────────────────────────

O envelope espelha os dados essenciais do ExecutionContext para que
consumidores possam reconstruir contexto sem acesso ao ExecutionContext
original:

✅ CorrelationId — distributed tracing entre produtor e consumidor
✅ TenantCode — roteamento multi-tenant
✅ ExecutionUser — audit trail (quem disparou)
✅ ExecutionOrigin — audit trail (de onde veio)
✅ BusinessOperationCode — qual operação de negócio

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Guid Para MessageId, Não Id do Domínio
───────────────────────────────────────────────────────────────────────────────

O envelope usa Guid (primitivo), não Bedrock.Core.Ids.Id (value object):
✅ Guid MessageId — serializável sem dependência de domínio
❌ Id MessageId — força consumidores a referenciar BuildingBlocks.Core

RAZÃO: Mensagens cruzam fronteiras. O envelope deve ser o mais portátil
possível. Tipos de domínio ficam no payload.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Record Selado — Não Herdável
───────────────────────────────────────────────────────────────────────────────

MessageMetadata é sealed record: imutável, igualdade por valor, e não
permite herança. Extensões futuras do envelope adicionam propriedades
aqui — todos os tipos de mensagem ganham automaticamente.

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Standard message envelope containing metadata for routing, tracing, and multi-tenancy.
/// Designed for independent deserialization before the typed payload.
/// </summary>
public sealed record MessageMetadata(
    Guid MessageId,
    DateTimeOffset Timestamp,
    string SchemaName,
    Guid CorrelationId,
    Guid TenantCode,
    string ExecutionUser,
    string ExecutionOrigin,
    string BusinessOperationCode
);
