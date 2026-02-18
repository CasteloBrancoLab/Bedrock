using Bedrock.BuildingBlocks.Security.Passwords.Interfaces;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using ShopDemo.Auth.Domain.Services.Interfaces;

namespace ShopDemo.Auth.Domain.Services;

public sealed class ClientCredentialsService : IClientCredentialsService
{
    private readonly IServiceClientRepository _serviceClientRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ClientCredentialsService(
        IServiceClientRepository serviceClientRepository,
        IPasswordHasher passwordHasher
    )
    {
        _serviceClientRepository = serviceClientRepository ?? throw new ArgumentNullException(nameof(serviceClientRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<ServiceClient?> ValidateCredentialsAsync(
        ExecutionContext executionContext,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        ServiceClient? serviceClient = await _serviceClientRepository.GetByClientIdAsync(
            executionContext,
            clientId,
            cancellationToken);

        if (serviceClient is null)
            return null;

        if (serviceClient.Status != ServiceClientStatus.Active)
            return null;

        if (serviceClient.ExpiresAt.HasValue && serviceClient.ExpiresAt.Value < executionContext.Timestamp)
            return null;

        var verificationResult = _passwordHasher.VerifyPassword(
            executionContext,
            clientSecret,
            serviceClient.ClientSecretHash);

        if (!verificationResult.IsValid)
            return null;

        return serviceClient;
    }
}
