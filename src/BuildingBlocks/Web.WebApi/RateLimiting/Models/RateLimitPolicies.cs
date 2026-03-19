using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace Bedrock.BuildingBlocks.Web.WebApi.RateLimiting.Models;

internal sealed record GlobalRateLimitPolicy(
    int PermitLimit,
    TimeSpan Window,
    Action<SlidingWindowRateLimiterOptions>? Configure);

internal sealed record TenantRateLimitPolicy(
    int PermitLimit,
    TimeSpan Window,
    Action<SlidingWindowRateLimiterOptions>? Configure);

internal sealed record RouteRateLimitPolicy(
    string PolicyName,
    int PermitLimit,
    TimeSpan Window,
    Action<SlidingWindowRateLimiterOptions>? Configure);

internal sealed record CompositeKeyRateLimitPolicy(
    string PolicyName,
    int PermitLimit,
    TimeSpan Window,
    Func<HttpContext, string> PartitionKeyFactory,
    Action<SlidingWindowRateLimiterOptions>? Configure);
