namespace Bedrock.BuildingBlocks.Web.Logging.Models;

// Configuracao fluente para request logging do Bedrock.
//
// Gera logs estruturados para cada request HTTP com:
// - Correlation ID (do ExecutionContext)
// - Elapsed time
// - HTTP method, path, status code
// - Headers sensiveis mascarados automaticamente
//
// Body logging e desabilitado por default (impacto em performance e privacidade).
// Paths de infraestrutura (health, docs) sao excluidos por default.
//
// Uso tipico:
//   new BedrockRequestLoggingOptions()
//       .WithSensitiveHeaders("X-Custom-Token")
//       .ExcludePaths("/health/*", "/scalar/*")
//       .WithRequestBodyLogging(maxLength: 4096)
public sealed class BedrockRequestLoggingOptions
{
    internal HashSet<string> SensitiveHeaders { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key",
    };

    internal HashSet<string> ExcludedPaths { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/swagger",
        "/scalar",
    };

    internal int? RequestBodyMaxLength { get; private set; }
    internal int? ResponseBodyMaxLength { get; private set; }

    // Adiciona headers que devem ser mascarados nos logs.
    // Os defaults (Authorization, Cookie, Set-Cookie, X-Api-Key) sao mantidos.
    public BedrockRequestLoggingOptions WithSensitiveHeaders(params string[] headers)
    {
        foreach (var header in headers)
        {
            SensitiveHeaders.Add(header);
        }

        return this;
    }

    // Adiciona paths que devem ser excluidos do logging.
    // Aceita prefixos (ex: "/health" exclui "/health/ready", "/health/live").
    // Os defaults (/health, /swagger, /scalar) sao mantidos.
    public BedrockRequestLoggingOptions ExcludePaths(params string[] paths)
    {
        foreach (var path in paths)
        {
            ExcludedPaths.Add(path.TrimEnd('/', '*'));
        }

        return this;
    }

    // Habilita logging do corpo do request (desabilitado por default).
    // maxLength limita o tamanho capturado em bytes para evitar overhead em payloads grandes.
    public BedrockRequestLoggingOptions WithRequestBodyLogging(int maxLength = 4096)
    {
        RequestBodyMaxLength = maxLength;
        return this;
    }

    // Habilita logging do corpo do response (desabilitado por default).
    // maxLength limita o tamanho capturado em bytes.
    public BedrockRequestLoggingOptions WithResponseBodyLogging(int maxLength = 4096)
    {
        ResponseBodyMaxLength = maxLength;
        return this;
    }
}
