namespace Bedrock.BuildingBlocks.Web.WebApi.Envelope.Models;

public sealed record ResponseMessage(
    ResponseMessageType Type,
    string Code,
    string? Description
);
