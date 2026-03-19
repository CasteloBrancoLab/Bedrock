namespace Bedrock.BuildingBlocks.Web.WebApi.Envelope.Models;

public sealed record ApiPayload<T>(T Data);
