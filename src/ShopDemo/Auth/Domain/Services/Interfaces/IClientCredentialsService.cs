using ShopDemo.Auth.Domain.Entities.ServiceClients;

namespace ShopDemo.Auth.Domain.Services.Interfaces;

public interface IClientCredentialsService
{
    Task<ServiceClient?> ValidateCredentialsAsync(
        ExecutionContext executionContext,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken);
}
