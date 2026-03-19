using Microsoft.AspNetCore.OutputCaching;

namespace Bedrock.BuildingBlocks.Web.WebApi.OutputCaching.Models;

internal sealed record OutputCachePolicy(
    string Name,
    TimeSpan? Duration,
    bool VaryByQuery,
    Action<OutputCachePolicyBuilder>? Configure);
