using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Outbox.Interfaces;
using Bedrock.BuildingBlocks.Serialization.Abstractions.Interfaces;

namespace Bedrock.BuildingBlocks.Outbox.Messages;

/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Serializacao via IStringSerializer.SerializeToUtf8Bytes
───────────────────────────────────────────────────────────────────────────────

Usa o IStringSerializer (JSON) do BuildingBlocks.Serialization para
serializar a message como UTF-8 bytes. O resultado e gravado diretamente
no bytea do PostgreSQL sem conversao intermediaria.

O tipo concreto da message e passado via GetType() para que o serializer
inclua todos os campos do record posicional (nao apenas os de MessageBase).

───────────────────────────────────────────────────────────────────────────────
*/

/// <summary>
/// Serializa <see cref="MessageBase"/> para bytes usando <see cref="IStringSerializer"/>.
/// Produz JSON em UTF-8, pronto para armazenamento como bytea.
/// </summary>
public sealed class MessageOutboxSerializer : IOutboxSerializer<MessageBase>
{
    private readonly IStringSerializer _serializer;

    public MessageOutboxSerializer(IStringSerializer serializer)
    {
        _serializer = serializer;
    }

    /// <inheritdoc />
    public string ContentType => "application/json";

    /// <inheritdoc />
    public byte[] Serialize(MessageBase payload)
    {
        // Serializar usando o tipo concreto (ex: UserRegisteredEvent),
        // nao MessageBase, para incluir todos os parametros posicionais
        var concreteType = payload.GetType();
        return _serializer.SerializeToUtf8Bytes(payload, concreteType)
               ?? throw new InvalidOperationException(
                   $"Serialization of {concreteType.FullName} returned null.");
    }
}
