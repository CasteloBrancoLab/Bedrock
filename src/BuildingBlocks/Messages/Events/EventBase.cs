using Bedrock.BuildingBlocks.Messages.Events.Interfaces;

namespace Bedrock.BuildingBlocks.Messages.Events;

/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Abstract Record Com Metadata Encapsulado
───────────────────────────────────────────────────────────────────────────────

EventBase recebe MessageMetadata e repassa para MessageBase.
Tipos concretos herdam e adicionam payload:

public sealed record UserRegisteredEvent(
    MessageMetadata Metadata,
    Id UserId, string Email
) : EventBase(Metadata);

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Nomenclatura - Passado Simples
───────────────────────────────────────────────────────────────────────────────

Events representam fatos que JÁ aconteceram. Use passado:
✅ UserRegisteredEvent, OrderCancelledEvent, NameChangedEvent
❌ RegisterUserEvent, CancelOrderEvent // imperativo = comando, não evento

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: SchemaName Auto-Computado — Nunca Passado Manualmente
───────────────────────────────────────────────────────────────────────────────

SchemaName é computado por MessageBase via GetType().FullName e injetado
no Metadata automaticamente. Tipos concretos NÃO preenchem SchemaName.

───────────────────────────────────────────────────────────────────────────────
*/

/// <summary>
/// Abstract base record for events, providing the standard message envelope.
/// </summary>
public abstract record EventBase(MessageMetadata Metadata)
    : MessageBase(Metadata), IEvent;
