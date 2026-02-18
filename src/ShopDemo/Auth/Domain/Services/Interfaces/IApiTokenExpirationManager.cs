using ShopDemo.Auth.Domain.Entities.ServiceClients;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IApiTokenExpirationManager
{
    DateTimeOffset CalculateExpiration(
        ExecutionContext executionContext,
        int? requestedTtlDays);

    bool IsExpired(
        ExecutionContext executionContext,
        ServiceClient serviceClient);

    bool IsNearExpiration(
        ExecutionContext executionContext,
        ServiceClient serviceClient);
}
