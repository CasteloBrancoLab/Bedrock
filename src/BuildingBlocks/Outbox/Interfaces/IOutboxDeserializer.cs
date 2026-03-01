namespace Bedrock.BuildingBlocks.Outbox.Interfaces;

/// <summary>
/// Deserializa entries da outbox de volta para objetos tipados.
/// Usa payloadType (discriminador) e contentType para resolver o tipo concreto.
/// </summary>
public interface IOutboxDeserializer
{
    /// <summary>
    /// Deserializa bytes de volta para o objeto original.
    /// </summary>
    /// <param name="data">Bytes serializados.</param>
    /// <param name="payloadType">Discriminador do tipo (ex: SchemaName).</param>
    /// <param name="contentType">Tipo de conteudo (ex: "application/json").</param>
    /// <returns>Objeto deserializado.</returns>
    object? Deserialize(byte[] data, string payloadType, string contentType);
}
