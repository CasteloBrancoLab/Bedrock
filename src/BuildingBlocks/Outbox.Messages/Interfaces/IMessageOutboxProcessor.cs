using Bedrock.BuildingBlocks.Outbox.Interfaces;

namespace Bedrock.BuildingBlocks.Outbox.Messages.Interfaces;

/// <summary>
/// Processador de outbox especializado para messages.
/// Deserializa entries da outbox de volta para MessageBase e publica no broker.
/// </summary>
public interface IMessageOutboxProcessor : IOutboxProcessor;
