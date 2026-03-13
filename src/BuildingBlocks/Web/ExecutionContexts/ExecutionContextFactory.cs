using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Microsoft.AspNetCore.Http;

namespace Bedrock.BuildingBlocks.Web.ExecutionContexts;

// Extrai campos do ExecutionContext a partir de headers HTTP.
//
// Em arquitetura com API gateway, o gateway valida o JWT e propaga
// os campos principais como headers de conveniencia para os servicos internos.
// Servicos internos confiam nos headers (rede privada) e nao validam o JWT.
//
// O header Authorization e propagado pelo gateway para que servicos internos
// possam extrair claims adicionais se necessario (sem validar assinatura/expiracao).
public sealed class ExecutionContextFactory : IExecutionContextFactory
{
    public const string CorrelationIdHeaderName = "X-Correlation-Id";
    public const string TenantIdHeaderName = "X-Tenant-Id";
    public const string ExecutionUserHeaderName = "X-Execution-User";
    public const string ExecutionOriginHeaderName = "X-Execution-Origin";
    public const string BusinessOperationCodeHeaderName = "X-Business-Operation-Code";
    public const string DefaultExecutionOrigin = "Api";
    public const string DefaultExecutionUser = "anonymous";

    private readonly TimeProvider _timeProvider;

    public ExecutionContextFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public ExecutionContext Create(HttpContext httpContext, string businessOperationCode)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(businessOperationCode);

        return ExecutionContext.Create(
            correlationId: ExtractCorrelationId(httpContext),
            tenantInfo: ExtractTenantInfo(httpContext),
            executionUser: ExtractExecutionUser(httpContext),
            executionOrigin: ExtractExecutionOrigin(httpContext),
            businessOperationCode: ExtractBusinessOperationCode(httpContext, businessOperationCode),
            minimumMessageType: MessageType.Information,
            timeProvider: _timeProvider);
    }

    // Auto-gera CorrelationId se o header nao for fornecido,
    // garantindo rastreabilidade mesmo para chamadas sem gateway.
    private static Guid ExtractCorrelationId(HttpContext httpContext)
    {
        return Guid.TryParse(
            httpContext.Request.Headers[CorrelationIdHeaderName].FirstOrDefault(),
            out var parsed)
            ? parsed
            : Guid.NewGuid();
    }

    private static TenantInfo ExtractTenantInfo(HttpContext httpContext)
    {
        var tenantId = Guid.TryParse(
            httpContext.Request.Headers[TenantIdHeaderName].FirstOrDefault(),
            out var parsed)
            ? parsed
            : Guid.Empty;

        return TenantInfo.Create(tenantId);
    }

    // Prioridade: header X-Execution-User (propagado pelo gateway) > ClaimsPrincipal
    // (para cenarios onde o servico recebe o JWT direto) > fallback "anonymous".
    private static string ExtractExecutionUser(HttpContext httpContext)
    {
        var headerValue = httpContext.Request.Headers[ExecutionUserHeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue;
        }

        return httpContext.User.Identity?.Name ?? DefaultExecutionUser;
    }

    // Permite que o gateway identifique a origem da chamada (web, mobile, service, etc.).
    // Fallback "Api" para chamadas diretas sem gateway.
    private static string ExtractExecutionOrigin(HttpContext httpContext)
    {
        var headerValue = httpContext.Request.Headers[ExecutionOriginHeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue;
        }

        return DefaultExecutionOrigin;
    }

    // Prioridade: header X-Business-Operation-Code (propagado pelo servico iniciador)
    // > argumento local (definido na action da controller).
    // Quando o servico e o iniciador do processo, o argumento local define o codigo.
    // Quando e chamado por outro servico, o header propaga o codigo original,
    // permitindo observabilidade end-to-end do use case em toda a cadeia.
    private static string ExtractBusinessOperationCode(HttpContext httpContext, string fallback)
    {
        var headerValue = httpContext.Request.Headers[BusinessOperationCodeHeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue;
        }

        return fallback;
    }
}
