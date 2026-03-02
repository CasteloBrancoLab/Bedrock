namespace Bedrock.BuildingBlocks.Outbox.Interfaces;

/// <summary>
/// Serializa payloads para armazenamento na outbox.
/// Cada especializacao (Messages, Webhooks, etc.) implementa sua propria serializacao.
/// </summary>
/// <typeparam name="TPayload">Tipo do payload a ser serializado.</typeparam>
public interface IOutboxSerializer<in TPayload>
{
    /// <summary>
    /// Tipo de conteudo produzido pela serializacao (ex: "application/json").
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Serializa o payload para bytes.
    /// </summary>
    /// <param name="payload">Payload a serializar.</param>
    /// <returns>Bytes serializados.</returns>
    byte[] Serialize(TPayload payload);
}
