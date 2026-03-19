using Bedrock.BuildingBlocks.Web.Hosting.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Bedrock.BuildingBlocks.Web.Hosting.Extensions;

public static class WebApplicationBuilderExtensions
{
    // Configura o Kestrel com os endpoints definidos no BedrockKestrelOptions.
    // Cada endpoint registra um ListenAnyIP com porta e protocolo, e o callback
    // opcional permite estender o ListenOptions individualmente (HTTPS, certificados,
    // limites de conexao, etc.). O Configure global aplica-se ao KestrelServerOptions
    // apos todos os endpoints serem registrados.
    public static WebApplicationBuilder UseBedrockKestrel(
        this WebApplicationBuilder builder,
        BedrockKestrelOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        builder.WebHost.ConfigureKestrel(kestrel =>
        {
            // Remove o header "Server: Kestrel" das respostas para evitar
            // information disclosure sobre a tecnologia do servidor.
            kestrel.AddServerHeader = false;

            foreach (var endpoint in options.Endpoints)
            {
                kestrel.ListenAnyIP(endpoint.Port, listenOptions =>
                {
                    listenOptions.Protocols = endpoint.Protocols;
                    endpoint.Configure?.Invoke(listenOptions);
                });
            }

            options.Configure?.Invoke(kestrel);
        });

        return builder;
    }
}
