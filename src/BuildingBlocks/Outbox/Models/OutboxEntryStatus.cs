namespace Bedrock.BuildingBlocks.Outbox.Models;

/// <summary>
/// Status do ciclo de vida de uma entry na outbox.
/// </summary>
public enum OutboxEntryStatus : byte
{
    /// <summary>
    /// Aguardando processamento. Estado inicial.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Em processamento por um worker (lease ativo).
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Enviado com sucesso ao destino.
    /// </summary>
    Sent = 3,

    /// <summary>
    /// Falhou apos tentativa de envio. Sera retentado.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Falhou apos exceder o limite de retentativas. Requer intervencao manual.
    /// </summary>
    Dead = 5
}
