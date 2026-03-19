namespace Bedrock.BuildingBlocks.Web.WebApi.Cors.Models;

// Configuracao de uma politica CORS individual.
// Cada politica define origens, metodos e headers permitidos
// para um conjunto especifico de endpoints.
public sealed class BedrockCorsPolicyOptions
{
    internal string Name { get; }
    internal List<string> Origins { get; } = [];
    internal List<string> Methods { get; } = [];
    internal List<string> Headers { get; } = [];
    internal List<string> ExposedHeaders { get; } = [];
    internal bool AllowCredentials { get; private set; }
    internal TimeSpan? PreflightMaxAge { get; private set; }

    internal BedrockCorsPolicyOptions(string name)
    {
        Name = name;
    }

    // Origens permitidas (ex: "https://app.example.com").
    // Cada origem deve ser um URL completo com scheme e host.
    // Nao use "*" com AllowCredentials — o browser bloqueia.
    public BedrockCorsPolicyOptions WithOrigins(params string[] origins)
    {
        Origins.AddRange(origins);
        return this;
    }

    // Metodos HTTP permitidos (ex: "GET", "POST", "PUT", "DELETE").
    // Se nao configurado, o default do .NET permite metodos simples (GET, POST, HEAD).
    public BedrockCorsPolicyOptions WithMethods(params string[] methods)
    {
        Methods.AddRange(methods);
        return this;
    }

    // Headers permitidos em requests cross-origin.
    // Util para headers customizados como X-Correlation-Id, X-Tenant-Id, etc.
    public BedrockCorsPolicyOptions WithHeaders(params string[] headers)
    {
        Headers.AddRange(headers);
        return this;
    }

    // Headers expostos na response que o browser pode ler via JavaScript.
    // Por default, o browser so expoe headers CORS-safelisted.
    // Util para expor headers como Retry-After, X-Request-Id, etc.
    public BedrockCorsPolicyOptions WithExposedHeaders(params string[] headers)
    {
        ExposedHeaders.AddRange(headers);
        return this;
    }

    // Permite envio de cookies e Authorization header em requests cross-origin.
    // Requer origens explicitas — nao funciona com AllowAnyOrigin.
    public BedrockCorsPolicyOptions WithCredentials()
    {
        AllowCredentials = true;
        return this;
    }

    // Tempo que o browser cacheia o resultado do preflight (OPTIONS).
    // Reduz requests OPTIONS repetidos para o mesmo endpoint.
    // Default recomendado: 1 hora. Maximo suportado por browsers: 2 horas.
    public BedrockCorsPolicyOptions WithPreflightMaxAge(TimeSpan maxAge)
    {
        PreflightMaxAge = maxAge;
        return this;
    }
}
