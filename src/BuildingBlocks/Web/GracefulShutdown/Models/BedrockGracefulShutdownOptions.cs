namespace Bedrock.BuildingBlocks.Web.GracefulShutdown.Models;

// Configuracao fluente para graceful shutdown do Bedrock.
//
// Permite definir timeout maximo e callbacks ordenados que executam
// quando o host recebe sinal de shutdown (SIGTERM, Ctrl+C, etc.).
//
// Os callbacks sao executados na ordem em que foram registrados,
// recebem o IServiceProvider e CancellationToken do shutdown.
//
// Uso tipico:
//   new BedrockGracefulShutdownOptions()
//       .WithTimeout(TimeSpan.FromSeconds(30))
//       .OnShutdown(async (sp, ct) => { /* flush buffers */ })
//       .OnShutdown(async (sp, ct) => { /* close connections */ })
public sealed class BedrockGracefulShutdownOptions
{
    internal TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(30);
    internal List<Func<IServiceProvider, CancellationToken, Task>> ShutdownCallbacks { get; } = [];

    // Define o timeout maximo para o graceful shutdown.
    // Apos esse periodo, o host forca o encerramento.
    // Default: 30 segundos.
    public BedrockGracefulShutdownOptions WithTimeout(TimeSpan timeout)
    {
        Timeout = timeout;
        return this;
    }

    // Registra um callback que sera executado durante o shutdown.
    // Callbacks sao executados na ordem de registro.
    // O CancellationToken e cancelado quando o timeout expira.
    public BedrockGracefulShutdownOptions OnShutdown(
        Func<IServiceProvider, CancellationToken, Task> callback)
    {
        ShutdownCallbacks.Add(callback);
        return this;
    }
}
