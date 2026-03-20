using Microsoft.AspNetCore.Http;

namespace Bedrock.BuildingBlocks.Web.Security.Middlewares;

// Adiciona headers de seguranca padrao em todas as respostas HTTP.
// Esses headers sao recomendacoes da OWASP para prevenir classes comuns
// de ataques em aplicacoes web. O callback opcional permite sobrescrever
// ou adicionar headers apos os defaults (ex: CSP customizado).
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Action<IHeaderDictionary>? _configure;

    public SecurityHeadersMiddleware(RequestDelegate next, Action<IHeaderDictionary>? configure = null)
    {
        _next = next;
        _configure = configure;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Previne MIME-type sniffing — forca o browser a respeitar o Content-Type
        // declarado, impedindo que interprete respostas como tipo diferente.
        headers["X-Content-Type-Options"] = "nosniff";

        // Previne clickjacking — impede que a pagina seja carregada em iframes,
        // bloqueando ataques que sobrepoem UI maliciosa sobre a aplicacao.
        headers["X-Frame-Options"] = "DENY";

        // Controla o header Referer em navegacoes cross-origin — evita vazamento
        // de URLs internas (que podem conter tokens, IDs sensiveis) para dominios externos.
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Desabilita o filtro XSS legado do browser. A recomendacao atual da OWASP
        // e desabilitar porque o proprio filtro pode introduzir vulnerabilidades
        // (XSS Auditor bypass). A protecao correta e via CSP.
        headers["X-XSS-Protection"] = "0";

        // Content Security Policy baseline para APIs — restringe todas as origens
        // de conteudo ao proprio dominio. APIs nao servem HTML, mas o header
        // protege contra respostas interpretadas indevidamente pelo browser.
        headers["Content-Security-Policy"] = "default-src 'self'";

        _configure?.Invoke(headers);

        return _next(context);
    }
}
