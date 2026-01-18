using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using CoreExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;

namespace Templates.Infra.CrossCutting.Bootstrapper;

public static class Startup
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private const string TenantCodeHeader = "X-Tenant-Code";
    private const string TenantNameHeader = "X-Tenant-Name";
    private const string ExecutionUserHeader = "X-Execution-User";
    private const string ExecutionOriginHeader = "X-Execution-Origin";
    private const string BusinessOperationCodeHeader = "X-Business-Operation-Code";

    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();

        services.AddScoped(sp =>
        {
            var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext
                ?? throw new InvalidOperationException("HttpContext not available.");

            var timeProvider = sp.GetRequiredService<TimeProvider>();

            var correlationId = ExtractGuidHeader(httpContext, CorrelationIdHeader) ?? Guid.NewGuid();
            var tenantCode = ExtractGuidHeader(httpContext, TenantCodeHeader) ?? Guid.Empty;
            var tenantName = ExtractHeader(httpContext, TenantNameHeader);
            var executionUser = ExtractHeader(httpContext, ExecutionUserHeader) ?? "anonymous";
            var executionOrigin = ExtractHeader(httpContext, ExecutionOriginHeader) ?? httpContext.Request.Path.Value ?? "unknown";
            var businessOperationCode = ExtractHeader(httpContext, BusinessOperationCodeHeader) ?? $"{httpContext.Request.Method}:{httpContext.Request.Path}";

            return CoreExecutionContext.Create(
                correlationId: correlationId,
                tenantInfo: TenantInfo.Create(tenantCode, tenantName),
                executionUser: executionUser,
                executionOrigin: executionOrigin,
                businessOperationCode: businessOperationCode,
                minimumMessageType: MessageType.Information,
                timeProvider: timeProvider
            );
        });

        return services;
    }

    private static string? ExtractHeader(HttpContext httpContext, string headerName)
    {
        if (httpContext.Request.Headers.TryGetValue(headerName, out var value) &&
            !string.IsNullOrWhiteSpace(value.ToString()))
        {
            return value.ToString();
        }
        return null;
    }

    private static Guid? ExtractGuidHeader(HttpContext httpContext, string headerName)
    {
        var value = ExtractHeader(httpContext, headerName);
        if (value is not null && Guid.TryParse(value, out var guid))
        {
            return guid;
        }
        return null;
    }
}
