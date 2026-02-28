using Bedrock.BuildingBlocks.Messages.Interfaces;

namespace Bedrock.BuildingBlocks.Messages.Events.Interfaces;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Domain Events - Notificação de Mudanças de Estado
═══════════════════════════════════════════════════════════════════════════════

Domain Events representam fatos que aconteceram no domínio.
São imutáveis, carregam apenas dados, e não contêm lógica.

Tipos concretos ficam em V1/Events/, V2/Events/, etc.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Event Herda de EventBase
───────────────────────────────────────────────────────────────────────────────

Todo event concreto herda de EventBase (que herda de MessageBase).
O envelope (Metadata) é herdado. O tipo concreto adiciona apenas payload:

✅ public sealed record UserRegisteredEvent(
       MessageMetadata Metadata,
       Id UserId, string Email
   ) : EventBase(Metadata);

❌ public readonly record struct UserRegisteredEvent(Id UserId) // sem envelope

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Dados Suficientes Para o Consumidor Agir
───────────────────────────────────────────────────────────────────────────────

O event deve carregar dados suficientes para que o consumidor possa reagir
SEM precisar consultar o repositório novamente:

✅ SimpleAggregateRootRegisteredEvent(Metadata, Id AggregateRootId, string FullName)
❌ SimpleAggregateRootRegisteredEvent(Metadata, Id AggregateRootId) // consumidor precisa GetById

RAZÃO: Evitar round-trips desnecessários. O produtor já tem os dados.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Nomenclatura - Passado Simples
───────────────────────────────────────────────────────────────────────────────

Events representam fatos que JÁ aconteceram. Use passado:
✅ UserRegisteredEvent, OrderCancelledEvent, NameChangedEvent
❌ RegisterUserEvent, CancelOrderEvent // imperativo = comando, não evento

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Marker interface for all domain events in this bounded context.
/// </summary>
public interface IEvent : IMessage;
