using Microsoft.Extensions.Http.Resilience;

namespace Bedrock.BuildingBlocks.Web.WebApi.Resilience.Models;

internal sealed record ResiliencePipeline(
    string HttpClientName,
    bool IsStandard,
    Action<HttpStandardResilienceOptions>? Configure);
