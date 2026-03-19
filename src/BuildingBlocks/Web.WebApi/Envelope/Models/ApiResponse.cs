namespace Bedrock.BuildingBlocks.Web.WebApi.Envelope.Models;

public sealed record ApiResponse<T>(
    T? Data,
    IReadOnlyList<ResponseMessage> Messages,
    Guid CorrelationId,
    DateTimeOffset Timestamp,
    string Language
) : IApiResponse
{
    IApiResponse IApiResponse.WithTranslatedMessages(IReadOnlyList<ResponseMessage> messages)
        => this with { Messages = messages };
}
