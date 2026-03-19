using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models;
using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Web.WebApi.Envelope.Models;

namespace Bedrock.BuildingBlocks.Web.WebApi.Envelope.Factories;

// Constroi ApiResponse<T> a partir do ExecutionContext, mapeando messages
// e preenchendo correlationId/timestamp/language automaticamente.
public static class ApiResponseFactory
{
    public static ApiResponse<T> Create<T>(T? data, ExecutionContext executionContext)
    {
        return new ApiResponse<T>(
            Data: data,
            Messages: MapMessages(executionContext),
            CorrelationId: executionContext.CorrelationId,
            Timestamp: executionContext.Timestamp,
            Language: executionContext.Language
        );
    }

    public static ApiResponse<T> CreateEmpty<T>(ExecutionContext executionContext)
    {
        return Create<T>(default, executionContext);
    }

    private static IReadOnlyList<ResponseMessage> MapMessages(ExecutionContext executionContext)
    {
        var messages = new List<ResponseMessage>();

        foreach (Message message in executionContext.Messages)
        {
            if (!ShouldIncludeInResponse(message.MessageType))
                continue;

            messages.Add(new ResponseMessage(
                Type: MapMessageType(message.MessageType),
                Code: message.Code,
                Description: message.Text
            ));
        }

        return messages;
    }

    private static bool ShouldIncludeInResponse(MessageType messageType)
    {
        return messageType is not (MessageType.Trace or MessageType.Debug or MessageType.None);
    }

    private static ResponseMessageType MapMessageType(MessageType messageType)
    {
        return messageType switch
        {
            MessageType.Information => ResponseMessageType.Information,
            MessageType.Warning => ResponseMessageType.Warning,
            MessageType.Error => ResponseMessageType.Error,
            MessageType.Critical => ResponseMessageType.Error,
            MessageType.Success => ResponseMessageType.Success,
            _ => ResponseMessageType.Information
        };
    }
}
