using Bedrock.BuildingBlocks.Outbox.Interfaces;
using Bedrock.BuildingBlocks.Serialization.Abstractions.Interfaces;

namespace Bedrock.BuildingBlocks.Outbox.Messages;

/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Deserializacao em Dois Estagios (MS-004)
───────────────────────────────────────────────────────────────────────────────

1. payloadType contem o SchemaName (full type name do tipo concreto)
2. Resolve o Type via Type.GetType()
3. Deserializa os bytes usando o IStringSerializer com o tipo resolvido

Isso permite reconstruir o tipo concreto (ex: UserRegisteredEvent) a partir
de bytes sem saber em compile-time qual tipo e.

───────────────────────────────────────────────────────────────────────────────
*/

/// <summary>
/// Deserializa entries da outbox de volta para messages concretas.
/// Usa o payloadType (SchemaName) para resolver o tipo e o
/// <see cref="IStringSerializer"/> para deserializar de UTF-8 bytes.
/// </summary>
public sealed class MessageOutboxDeserializer : IOutboxDeserializer
{
    private readonly IStringSerializer _serializer;

    public MessageOutboxDeserializer(IStringSerializer serializer)
    {
        _serializer = serializer;
    }

    /// <inheritdoc />
    public object? Deserialize(byte[] data, string payloadType, string contentType)
    {
        var type = Type.GetType(payloadType)
                   ?? throw new InvalidOperationException(
                       $"Cannot resolve type '{payloadType}'. " +
                       $"Ensure the assembly containing this type is loaded.");

        return _serializer.DeserializeFromUtf8Bytes<object>(data, type);
    }
}
