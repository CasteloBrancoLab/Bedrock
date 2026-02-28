using Bedrock.BuildingBlocks.Messages.Interfaces;

namespace Bedrock.BuildingBlocks.Messages;

/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Abstract Record Com Metadata Encapsulado
───────────────────────────────────────────────────────────────────────────────

MessageBase recebe MessageMetadata como parâmetro único de envelope.
Tipos concretos herdam e adicionam payload:

public sealed record UserRegisteredEvent(
    MessageMetadata Metadata,
    Id UserId, string Email
) : EventBase(Metadata);

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: SchemaName Auto-Computado — Injetado no Metadata
───────────────────────────────────────────────────────────────────────────────

O produtor NÃO precisa preencher SchemaName no Metadata.
MessageBase intercepta o Metadata recebido e sobrescreve SchemaName
com GetType().FullName via `with` expression (record copy).

Fluxo:
1. Produtor cria Metadata com SchemaName vazio/qualquer
2. MessageBase sobrescreve com o tipo concreto real
3. Consumer deserializa Metadata e lê SchemaName correto

✅ new UserRegisteredEvent(new MessageMetadata(..., SchemaName: "", ...), ...)
   → Metadata.SchemaName == "Namespace.V1.Events.UserRegisteredEvent"
❌ Passar SchemaName manualmente — error-prone e duplicado

───────────────────────────────────────────────────────────────────────────────
*/

/// <summary>
/// Abstract base record providing the standard message envelope.
/// SchemaName is auto-computed from the concrete type's full name and injected into Metadata.
/// </summary>
public abstract record MessageBase : IMessage
{
    /// <inheritdoc />
    public MessageMetadata Metadata { get; }

    protected MessageBase(MessageMetadata metadata)
    {
        Metadata = metadata with { SchemaName = GetType().FullName! };
    }
}
