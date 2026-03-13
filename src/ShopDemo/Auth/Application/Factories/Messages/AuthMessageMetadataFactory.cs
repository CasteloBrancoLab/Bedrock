using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Messages;

namespace ShopDemo.Auth.Application.Factories.Messages;

public static class AuthMessageMetadataFactory
{
    public static MessageMetadata Create(ExecutionContext executionContext, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(executionContext);
        ArgumentNullException.ThrowIfNull(timeProvider);

        return new MessageMetadata(
            MessageId: Id.GenerateNewId(timeProvider).Value,
            Timestamp: timeProvider.GetUtcNow(),
            SchemaName: string.Empty,
            CorrelationId: executionContext.CorrelationId,
            TenantCode: executionContext.TenantInfo.Code,
            ExecutionUser: executionContext.ExecutionUser,
            ExecutionOrigin: executionContext.ExecutionOrigin,
            BusinessOperationCode: executionContext.BusinessOperationCode);
    }
}
