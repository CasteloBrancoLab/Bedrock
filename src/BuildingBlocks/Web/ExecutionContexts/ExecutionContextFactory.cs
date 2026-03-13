using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Microsoft.AspNetCore.Http;

namespace Bedrock.BuildingBlocks.Web.ExecutionContexts;

public sealed class ExecutionContextFactory : IExecutionContextFactory
{
    public const string CorrelationIdHeaderName = "X-Correlation-Id";
    public const string TenantIdHeaderName = "X-Tenant-Id";
    public const string DefaultExecutionOrigin = "Api";

    private readonly TimeProvider _timeProvider;

    public ExecutionContextFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public ExecutionContext Create(HttpContext httpContext, string businessOperationCode)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(businessOperationCode);

        var correlationId = Guid.TryParse(
            httpContext.Request.Headers[CorrelationIdHeaderName].FirstOrDefault(),
            out var parsed)
            ? parsed
            : Guid.NewGuid();

        var tenantId = Guid.TryParse(
            httpContext.Request.Headers[TenantIdHeaderName].FirstOrDefault(),
            out var tenantParsed)
            ? tenantParsed
            : Guid.Empty;

        var executionUser = httpContext.User.Identity?.Name ?? "anonymous";

        return ExecutionContext.Create(
            correlationId: correlationId,
            tenantInfo: TenantInfo.Create(tenantId),
            executionUser: executionUser,
            executionOrigin: DefaultExecutionOrigin,
            businessOperationCode: businessOperationCode,
            minimumMessageType: MessageType.Information,
            timeProvider: _timeProvider);
    }
}
