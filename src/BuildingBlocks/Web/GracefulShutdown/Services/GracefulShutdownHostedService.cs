using Bedrock.BuildingBlocks.Web.GracefulShutdown.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bedrock.BuildingBlocks.Web.GracefulShutdown.Services;

// Hosted service que orquestra o graceful shutdown do Bedrock.
//
// Registra um callback em ApplicationStopping que executa todos os
// shutdown callbacks configurados, com timeout via CancellationToken.
// Se um callback falha, loga o erro e continua com os proximos.
internal sealed class GracefulShutdownHostedService : IHostedService
{
    private readonly BedrockGracefulShutdownOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<GracefulShutdownHostedService> _logger;

    public GracefulShutdownHostedService(
        BedrockGracefulShutdownOptions options,
        IServiceProvider serviceProvider,
        IHostApplicationLifetime lifetime,
        ILogger<GracefulShutdownHostedService> logger)
    {
        _options = options;
        _serviceProvider = serviceProvider;
        _lifetime = lifetime;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStopping.Register(() => ExecuteShutdownCallbacks().GetAwaiter().GetResult());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task ExecuteShutdownCallbacks()
    {
        if (_options.ShutdownCallbacks.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "Graceful shutdown initiated. Executing {Count} callback(s) with {Timeout}s timeout",
            _options.ShutdownCallbacks.Count,
            _options.Timeout.TotalSeconds);

        using var cts = new CancellationTokenSource(_options.Timeout);

        foreach (var callback in _options.ShutdownCallbacks)
        {
            try
            {
                await callback(_serviceProvider, cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Graceful shutdown timeout exceeded. Remaining callbacks skipped");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Graceful shutdown callback failed. Continuing with next callback");
            }
        }

        _logger.LogInformation("Graceful shutdown callbacks completed");
    }
}
