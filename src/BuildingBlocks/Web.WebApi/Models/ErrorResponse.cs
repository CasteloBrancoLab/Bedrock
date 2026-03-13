namespace Bedrock.BuildingBlocks.Web.WebApi.Models;

public sealed record ErrorResponse(
    string Code,
    string Message
);
