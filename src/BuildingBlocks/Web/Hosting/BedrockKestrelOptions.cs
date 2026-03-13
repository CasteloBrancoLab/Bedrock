using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Bedrock.BuildingBlocks.Web.Hosting;

// Opcoes de configuracao do Kestrel para o Bedrock.
// Cada endpoint e um Listen independente com porta, protocolo e callback
// para estender o ListenOptions (HTTPS, certificados, limites, etc.).
// O callback Configure permite estender o KestrelServerOptions globalmente.
public sealed class BedrockKestrelOptions
{
    private readonly List<BedrockKestrelEndpoint> _endpoints = [];

    public IReadOnlyList<BedrockKestrelEndpoint> Endpoints => _endpoints;
    public Action<KestrelServerOptions>? Configure { get; set; }

    public BedrockKestrelOptions Listen(
        int port,
        HttpProtocols protocols,
        Action<ListenOptions>? configure = null)
    {
        _endpoints.Add(new BedrockKestrelEndpoint(port, protocols, configure));
        return this;
    }
}

public sealed record BedrockKestrelEndpoint(
    int Port,
    HttpProtocols Protocols,
    Action<ListenOptions>? Configure
);
