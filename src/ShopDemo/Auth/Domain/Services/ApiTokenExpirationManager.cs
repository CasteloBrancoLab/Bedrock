using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class ApiTokenExpirationManager : IApiTokenExpirationManager
{
    public const int DefaultTtlDays = 90;
    public const int MaxTtlDays = 365;
    public const int NotificationDays = 7;

    public DateTimeOffset CalculateExpiration(
        ExecutionContext executionContext,
        int? requestedTtlDays)
    {
        int ttlDays = requestedTtlDays ?? DefaultTtlDays;

        if (ttlDays < 1)
        {
            executionContext.AddErrorMessage(
                code: "ApiTokenExpirationManager.TtlTooShort");
            ttlDays = DefaultTtlDays;
        }

        if (ttlDays > MaxTtlDays)
        {
            executionContext.AddErrorMessage(
                code: "ApiTokenExpirationManager.TtlTooLong");
            ttlDays = MaxTtlDays;
        }

        return executionContext.Timestamp.AddDays(ttlDays);
    }

    public bool IsExpired(
        ExecutionContext executionContext,
        ServiceClient serviceClient)
    {
        if (!serviceClient.ExpiresAt.HasValue)
            return false;

        return serviceClient.ExpiresAt.Value < executionContext.Timestamp;
    }

    public bool IsNearExpiration(
        ExecutionContext executionContext,
        ServiceClient serviceClient)
    {
        if (!serviceClient.ExpiresAt.HasValue)
            return false;

        DateTimeOffset notificationThreshold = serviceClient.ExpiresAt.Value.AddDays(-NotificationDays);

        return executionContext.Timestamp >= notificationThreshold
            && executionContext.Timestamp < serviceClient.ExpiresAt.Value;
    }
}
